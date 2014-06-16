using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Infrastructure.Edata
{
    public class EdataFileWriter : EdataWriterBase, IEdataFileWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileToWrite"></param>
        public void Write(EdataFile fileToWrite)
        {
            WriteContentInternal(fileToWrite);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileToWrite"></param>
        /// <param name="token"></param>
        public void Write(EdataFile fileToWrite, CancellationToken token)
        {
            WriteContentInternal(fileToWrite, token);
        }

        protected void WriteContentInternal(EdataFile edataFile, CancellationToken? token = null)
        {
            //Uważać tu na ścieżke jaka ma edata pełną czy relatywną..
            //No i dodatkowo od tego momentu może być pusta. A z racji tego że tylko możemy podmieniać edata nie pasuje
            //dodawać dodatkowy argument ścieżki do zapisu, bo to jasno wskazywało by że możemy zapisywać do dowolnej lokacji
            // a tak naprawde można tylko podmieniać edata.
            if (!File.Exists(edataFile.Path))
            {
                throw new ArgumentException(
                    String.Format("A following Edata file: \"{0}\" doesn't exist", edataFile.Path),
                    "edataFile");
            }

            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            //Wypieprzyć z tąd wszystkie zależności od starych metod, i stworzyć nowe metody zapisu poszczegołnych elementów
            //realizujące nową koncepcję wykorzystujaca model wpsiów słownika i być może w przyszłosci samego słownika.

            //W przypadku konieczności odbudowy słownika trzeba poszerzyć określenie czy można użyć ReplacemnetWrite
            //Jeśli nie ma nowego pliku, i słownik mieści sie w miejsce starego, to można użyć replacement write
            //Jeśli natomiast dodano nowy plik, lub słownik nie miesci sie na miejsce starego to trzeba zbudować plik Edata od zera.

            //Update na dobrą sprawę można założyć, że jeśli nie ma zmian w plikach to słownik zawszę będzie się mieścił w miejsce starego.
            if (!AnyContentFileExceedsAvailableSpace(edataFile) &&
                !edataFile.HasContentFilesCollectionChanged)
            {
                using (var sourceEdata = new FileStream(edataFile.Path, FileMode.Open))
                {
                    //Taka uwaga: Nie robić canceli w trakcie zapisu czy to nagłowka czy słownika, zeby w przypadku przerwania bez backapu zminimalizwoać
                    //            szanse na uszkodzenie modyifkowane go pliku.

                    //W tym przypadku używamy starego rozmieszczenia danych żeby nie odbudowywac pliku od nowa.
                    var header = edataFile.Header;

                    var dictEntries = CreateDictionaryEntriesOfContentFiles(edataFile.ContentFiles);
                    var dictOffset = header.DictOffset;
                    var dictLength = GetDictionaryLength(dictEntries);
                    var dictEnd = dictOffset + dictLength;

                    //Clear the old part of file up to content.
                    WritePadding(sourceEdata, 0, header.FileOffset);

                    WriteLoadedContentByReplace(sourceEdata, edataFile.ContentFiles);

                    AssignContentFilesInfoToDictEntries(edataFile.ContentFiles, dictEntries);

                    var dictWriteInfo = WriteDictionary(sourceEdata, dictEntries, dictOffset);

                    header.Checksum_V2 = Md5Hash.GetHash(dictWriteInfo.Checksum);
                    header.DictLength = dictWriteInfo.Length;

                    WriteHeader(sourceEdata, header, 0);
                }
            }
            else
            {
                //Try in the current dir to avoid double file moving
                String temporaryEdataPath = GetTemporaryEdataPathInCurrentLocation(edataFile.Path);
                if ((new FileInfo(edataFile.Path).Length > (new DriveInfo(temporaryEdataPath).AvailableFreeSpace)))
                {
                    temporaryEdataPath = TryGetTemporaryEdataPathWhereFree(edataFile.Path);
                    if (temporaryEdataPath == null)
                    {
                        throw new IOException(
                            String.Format("Not enough free disk space for rebuilding the \"{0}\" file.",
                            edataFile.Path));
                    }
                }

                //To avoid too many nested try catches.
                FileStream sourceEdata = null;
                FileStream newEdata = null;
                try
                {
                    sourceEdata = new FileStream(edataFile.Path, FileMode.Open);
                    newEdata = new FileStream(temporaryEdataPath, FileMode.Create);

                    //W tym przypadku rozmieszczamy wszystko od zera wg wartości obliczonych.
                    var dictEntries = CreateDictionaryEntriesOfContentFiles(edataFile.ContentFiles);
                    var dictOffset = GetDictionaryOffset();
                    var dictLength = GetDictionaryLength(dictEntries);
                    var dictEnd = dictOffset + dictLength;

                    var fileOffset = GetFileOffset(dictEnd);

                    WritePadding(newEdata, 0, fileOffset);

                    var contentWriteInfo = WriteNotLoadedContent(sourceEdata, newEdata, edataFile.ContentFiles, fileOffset);

                    AssignContentFilesInfoToDictEntries(edataFile.ContentFiles, dictEntries);

                    var dictWriteInfo = WriteDictionary(newEdata, dictEntries, dictOffset);

                    var header = edataFile.Header;
                    header.Checksum_V2 = Md5Hash.GetHash(dictWriteInfo.Checksum);
                    header.DictOffset = dictOffset;
                    header.DictLength = dictWriteInfo.Length;
                    header.FileOffset = fileOffset;
                    header.FileLenght = contentWriteInfo.Length;

                    WriteHeader(newEdata, header, 0);

                    //Free file handles before the file move and delete
                    CloseEdataFilesStreams(sourceEdata, newEdata);

                    //Replace temporary file
                    File.Delete(edataFile.Path);
                    File.Move(temporaryEdataPath, edataFile.Path);
                }
                finally
                {
                    //Spr czy zostały już zwolnione...?
                    CloseEdataFilesStreams(sourceEdata, newEdata);

                    if (File.Exists(temporaryEdataPath))
                    {
                        File.Delete(temporaryEdataPath);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected String GetTemporaryEdataPathInCurrentLocation(String oldeEdataPath)
        {
            var oldEdataFileInfo = new FileInfo(oldeEdataPath);
            var temporaryEdataPath = Path.Combine(
                oldEdataFileInfo.DirectoryName,
                Path.GetFileNameWithoutExtension(oldEdataFileInfo.Name) + ".tmp");

            return temporaryEdataPath;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldEdataPath"></param>
        /// <returns></returns>
        protected String TryGetTemporaryEdataPathWhereFree(String oldEdataPath)
        {
            var oldEdataFileInfo = new FileInfo(oldEdataPath);

            var fixedDrives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed);
            foreach (var drive in fixedDrives)
            {
                if (drive.AvailableFreeSpace > oldEdataFileInfo.Length)
                {
                    var tempFileName = Path.GetFileNameWithoutExtension(oldEdataFileInfo.Name) + ".tmp";
                    var temporaryEdataPath = Path.Combine(drive.Name, tempFileName);
                    //Create path if not exist
                    //PathUtilities.CreateDirectoryIfNotExist(temporaryEdataPath);

                    return temporaryEdataPath;
                }
            }

            return null;
        }

        //Zmienione z private na protected na razie
        protected void CloseEdataFilesStreams(FileStream sourceEdata, FileStream newEdata)
        {
            if (newEdata != null)
            {
                newEdata.Close();
            }

            if (sourceEdata != null)
            {
                sourceEdata.Close();
            }
        }

        //protected virtual void WriteHeaderV2(Stream target, EdataHeader header, uint offset)
        //{
        //    target.Seek(offset, SeekOrigin.Begin);

        //    byte[] rawHeader = WargameModInstaller.Common.Utilities.MiscUtilities.StructToBytes(header);
        //    target.Write(rawHeader, 0, rawHeader.Length);
        //}

        //protected virtual DictionaryWriteInfo WriteDictionary(
        //    Stream target,
        //    IEnumerable<EdataDictSubPath> entries,
        //    uint dictOffset)
        //{
        //    var info = new DictionaryWriteInfo();

        //    //WriteDictHeader
        //    target.Seek(dictOffset, SeekOrigin.Begin);

        //    byte[] buffer = { 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        //    target.Write(buffer, 0, buffer.Length);
        //    info.Length += (uint)buffer.Length;

        //    foreach (var rootEntry in entries)
        //    {
        //        //Need to use depth-traversal
        //        var entriesStack = new Stack<EdataDictSubPath>();
        //        entriesStack.Push(rootEntry);

        //        while (entriesStack.Count > 0)
        //        {
        //            var entry = entriesStack.Pop();

        //            var entryBuffer = entry.ToBytes();
        //            target.Write(entryBuffer, 0, entryBuffer.Length);
        //            info.Length += (uint)entryBuffer.Length;

        //            foreach (var subEntry in entry.FollowingSubPaths.Reverse())
        //            {
        //                entriesStack.Push(subEntry);
        //            }
        //        }
        //    }

        //    var dictBuff = new byte[info.Length];
        //    target.Seek(dictOffset, SeekOrigin.Begin);
        //    target.Read(dictBuff, 0, dictBuff.Length);

        //    info.Checksum = MD5.Create().ComputeHash(dictBuff);

        //    return info;
        //}

        //protected virtual ContentWriteInfo WriteLoadedContentV2(
        //    Stream target,
        //    IEnumerable<EdataContentFile> contentFiles,
        //    uint contentOffset)
        //{
        //    ContentWriteInfo info = new ContentWriteInfo();

        //    byte[] spaceBuffer = null;

        //    foreach (var file in contentFiles)
        //    {
        //        long oldOffset = file.Offset;
        //        file.Offset = target.Position - contentOffset;

        //        byte[] fileBuffer;

        //        fileBuffer = file.Content;
        //        file.Size = file.Content.Length;
        //        file.Checksum = MD5.Create().ComputeHash(fileBuffer);

        //        long spaceSize = GetSpaceSizeForFile(file);
        //        spaceBuffer = GetNewBufferIfNeeded(spaceBuffer, spaceSize);

        //        target.Write(fileBuffer, 0, fileBuffer.Length);
        //        target.Write(spaceBuffer, 0, (int)spaceSize);

        //        info.Length += (uint)fileBuffer.Length + (uint)spaceSize;
        //    }

        //    return info;
        //}

        //protected virtual ContentWriteInfo WriteNotLoadedContentV2(
        //    Stream source, 
        //    Stream target, 
        //    IEnumerable<EdataContentFile> contentFiles, 
        //    uint contentOffset)
        //{
        //    ContentWriteInfo info = new ContentWriteInfo();

        //    source.Seek(contentOffset, SeekOrigin.Begin);

        //    byte[] spaceBuffer = null;

        //    foreach (var file in contentFiles)
        //    {
        //        long oldOffset = file.Offset;
        //        file.Offset = target.Position - contentOffset;

        //        byte[] fileBuffer;

        //        if (file.IsContentLoaded)
        //        {
        //            fileBuffer = file.Content;
        //            file.Size = file.Content.Length; 
        //            file.Checksum = MD5.Create().ComputeHash(fileBuffer); 
        //        }
        //        else
        //        {
        //            fileBuffer = new byte[file.Size];
        //            source.Seek(oldOffset + contentOffset, SeekOrigin.Begin);
        //            source.Read(fileBuffer, 0, fileBuffer.Length);
        //        }

        //        long spaceSize = GetSpaceSizeForFile(file);
        //        spaceBuffer = GetNewBufferIfNeeded(spaceBuffer, spaceSize);

        //        target.Write(fileBuffer, 0, fileBuffer.Length);
        //        target.Write(spaceBuffer, 0, (int)spaceSize);

        //        info.Length += (uint)fileBuffer.Length + (uint)spaceSize;
        //    }

        //    return info;
        //}

        ////Nie zmienia długości contentu
        //protected virtual void WriteLoadedContentByReplace(
        //    Stream source, 
        //    IEnumerable<EdataContentFile> contentFiles)
        //{
        //    byte[] spaceBuffer = null;

        //    foreach (var contentFile in contentFiles)
        //    {
        //        if (contentFile.IsContentLoaded)
        //        {
        //            long orginalContentSize = contentFile.Size;

        //            byte[] fileBuffer = contentFile.Content;
        //            contentFile.Size = fileBuffer.Length;
        //            contentFile.Checksum = MD5.Create().ComputeHash(fileBuffer);

        //            source.Seek(contentFile.TotalOffset, SeekOrigin.Begin);
        //            source.Write(fileBuffer, 0, fileBuffer.Length);

        //            long contentSizeDffierence = orginalContentSize - contentFile.ContentSize;
        //            if (contentSizeDffierence > 0)
        //            {
        //                spaceBuffer = GetNewBufferIfNeeded(spaceBuffer, contentSizeDffierence);
        //                source.Write(spaceBuffer, 0, (int)contentSizeDffierence);
        //            }
        //        }
        //    }
        //}

        //protected virtual void WritePadding(FileStream target, uint offset, uint length)
        //{
        //    byte[] buffer = new byte[length];

        //    target.Seek(offset, SeekOrigin.Begin);
        //    target.Write(buffer, 0, buffer.Length);
        //}

        //protected virtual void AssignContentFilesInfoToDictEntries(
        //    IEnumerable<EdataContentFile> contentFiles,
        //    IEnumerable<EdataDictSubPath> dictEntries)
        //{
        //    foreach (var file in contentFiles)
        //    {
        //        EdataDictFileSubPath matchingFileEntry = null;
        //        foreach (var entry in dictEntries)
        //        {
        //            matchingFileEntry = (entry.SelectEntryByPath(file.Path) as EdataDictFileSubPath);
        //            if (matchingFileEntry != null)
        //            {
        //                break;
        //            }
        //        }

        //        if (matchingFileEntry != null)
        //        {
        //            matchingFileEntry.FileOffset = file.Offset;
        //            matchingFileEntry.FileLength = file.Size;
        //            matchingFileEntry.FileChecksum = file.Checksum;
        //        }
        //    }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="contentFiles"></param>
        ///// <returns></returns>
        //protected virtual IEnumerable<EdataDictSubPath> CreateDictionaryEntriesOfContentFiles(IEnumerable<EdataContentFile> contentFiles)
        //{
        //    //This alghoritm assumes that there are no two content files with the same paths.

        //    //Przenisc to sortowanie gdzies indziej, bo sie powtarza w innych metodach.
        //    var pathsToSplit = contentFiles
        //        .OrderBy(file => file.Path)
        //        .Select(file => new ContentPathSplitInfo() { Path = file.Path })
        //        .ToList();

        //    var dictionaryEntries = new List<EdataDictSubPath>();

        //    //Wide match can't be picked over a long match.
        //    while (pathsToSplit.Count > 0)
        //    {
        //        var pathsToCompare = GetPathsToCompare(pathsToSplit);
        //        if (pathsToCompare.Count == 1)
        //        {
        //            var newEntry = new EdataDictFileSubPath(pathsToCompare[0].GetPathFromSplitIndex());
        //            AddEntryToDictionary(newEntry, pathsToCompare[0], dictionaryEntries);

        //            pathsToSplit.Remove(pathsToCompare[0]);
        //        }
        //        else if (pathsToCompare.Count > 1)
        //        {
        //            int matchIndex = 0;

        //            while (true) //Zastanowić sie co z warunkiem kończoaczym, czy to break moze nie zaistnieć.
        //            {
        //                bool allPathsMatched = CheckIfAllPathsMatchAtIndex(pathsToCompare, matchIndex); ;
        //                if (allPathsMatched)
        //                {
        //                    matchIndex++;
        //                }
        //                else
        //                {
        //                    EdataDictDirSubPath newEntry = null;
        //                    if (String.IsNullOrEmpty(pathsToCompare[0].GetPathToSplitIndex()))
        //                    {
        //                        newEntry = new EdataDictRootDirSubPath(pathsToCompare[0].GetPathFromSplitIndex(length: matchIndex)); 
        //                    }
        //                    else
        //                    {
        //                        newEntry = new EdataDictDirSubPath(pathsToCompare[0].GetPathFromSplitIndex(length: matchIndex));
        //                    }
        //                    AddEntryToDictionary(newEntry, pathsToCompare[0], dictionaryEntries);

        //                    //czy tu powinno być += czy tylko przypisanie?
        //                    pathsToCompare.ForEach(x => x.SplitIndex += matchIndex);

        //                    break;
        //                }
        //            }
        //        }
        //    }

        //    return dictionaryEntries;
        //}

        ///// <summary>
        ///// Selects a list of paths to comparison from an another  list of paths. 
        ///// Paths which can be compared have an equal SplitIndex value, and start with the same first character.
        ///// </summary>
        ///// <param name="remainingSplitPaths"></param>
        ///// <returns></returns>
        //private List<ContentPathSplitInfo> GetPathsToCompare(IList<ContentPathSplitInfo> remainingSplitPaths)
        //{
        //    var pathsToCompare = new List<ContentPathSplitInfo>();

        //    var orgItem = remainingSplitPaths[0];
        //    pathsToCompare.Add(orgItem);

        //    //Zastąpić algorytmem porównującym z dzielenie obszaru porównywania. Czyli pierwsze porówniaie indeks 0 i ostatni.
        //    //jesli nie pasują to zero i pół itd, az do znalezienie dopasowania.
        //    for (int i = 1; i < remainingSplitPaths.Count; ++i)
        //    {
        //        var currentItem = remainingSplitPaths[i];

        //        if (currentItem.SplitIndex == orgItem.SplitIndex &&
        //            currentItem.GetCharAtSplitIndex() == orgItem.GetCharAtSplitIndex())
        //        {
        //            pathsToCompare.Add(remainingSplitPaths[i]);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }

        //    return pathsToCompare;
        //}

        ///// <summary>
        ///// Checks wheather paths in the given list, have the same character value at given index.
        ///// </summary>
        ///// <param name="comparedPaths"></param>
        ///// <param name="matchIndex"></param>
        ///// <returns></returns>
        //private bool CheckIfAllPathsMatchAtIndex(IList<ContentPathSplitInfo> comparedPaths, int matchIndex)
        //{
        //    var orgItem = comparedPaths[0];
        //    int orgIndex = orgItem.SplitIndex + matchIndex;

        //    for (int i = 1; i < comparedPaths.Count; ++i)
        //    {
        //        var currentItem = comparedPaths[i];
        //        int currentIndex = currentItem.SplitIndex + matchIndex;

        //        if (!(currentIndex == orgIndex &&
        //            currentIndex < currentItem.Path.Length &&
        //            orgIndex < orgItem.Path.Length &&
        //            currentItem.Path[currentIndex] == orgItem.Path[orgIndex]))
        //        {
        //            return false;
        //        }
        //    }

        //    return true;
        //}

        //private void AddEntryToDictionary(
        //    EdataDictSubPath entry,
        //    ContentPathSplitInfo entryPathSplitInfo,
        //    List<EdataDictSubPath> dictionaryEntries)
        //{
        //    //It's a first one so no need to search for a predecessor.
        //    if (dictionaryEntries.Count == 0)
        //    {
        //        dictionaryEntries.Add(entry);
        //        return;
        //    }

        //    var precedingPath = entryPathSplitInfo.GetPathToSplitIndex();
        //    EdataDictSubPath precedingPathEntry = null;
        //    foreach (var e in dictionaryEntries)
        //    {
        //        var matchingEntry = e.SelectEntryByPath(precedingPath);
        //        if (matchingEntry != null)
        //        {
        //            precedingPathEntry = matchingEntry;
        //            break;
        //        }
        //    }

        //    var pp = precedingPathEntry as EdataDictDirSubPath;
        //    if (precedingPathEntry != null)
        //    {
        //        pp.AddFollowingSubPath(entry);
        //        entry.PrecedingSubPath = pp;
        //    }
        //}

        //protected virtual uint GetDictionaryLength(IEnumerable<EdataDictSubPath> entries)
        //{
        //    uint dictHeaderSize = 10;
        //    uint totalSize = dictHeaderSize;

        //    foreach (var rootEntry in entries)
        //    {
        //        var entriesQueue = new Queue<EdataDictSubPath>();
        //        entriesQueue.Enqueue(rootEntry);

        //        while (entriesQueue.Count > 0)
        //        {
        //            var entry = entriesQueue.Dequeue();

        //            totalSize += entry.TotalLength;

        //            foreach (var subEntry in entry.FollowingSubPaths)
        //            {
        //                entriesQueue.Enqueue(subEntry);
        //            }
        //        }
        //    }

        //    return totalSize;
        //}

        //protected virtual uint GetHeaderOffset()
        //{
        //    return 0;
        //}

        //protected virtual uint GetDictionaryOffset()
        //{
        //    return 1037;
        //}

        //protected virtual uint GetFileOffset(uint dictionaryEnd)
        //{
        //    //Zdaje się że ta wrtość jest zapisana w nagłówku, obecnie pod nazwą Padding.
        //    uint offsetStep = 8192;
        //    uint fileOffset = 0;

        //    while (fileOffset <= dictionaryEnd)
        //    {
        //        fileOffset += offsetStep;
        //    }

        //    return fileOffset;
        //}

        //#region Nested classes

        //protected class DictionaryWriteInfo
        //{
        //    public byte[] Checksum { get; set; }
        //    public uint Length { get; set; }
        //}

        //protected class ContentWriteInfo
        //{
        //    public uint Length { get; set; }
        //}

        //protected class ContentPathSplitInfo
        //{
        //    public String Path { get; set; }
        //    public int SplitIndex { get; set; }

        //    public String GetPathToSplitIndex()
        //    {
        //        return Path.Substring(0, SplitIndex);
        //    }

        //    public String GetPathFromSplitIndex()
        //    {
        //        return Path.Substring(SplitIndex);
        //    }

        //    public String GetPathFromSplitIndex(int length)
        //    {
        //        return Path.Substring(SplitIndex, length);
        //    }

        //    public char GetCharAtSplitIndex()
        //    {
        //        return Path[SplitIndex];
        //    }

        //    public override String ToString()
        //    {
        //        return Path.ToString();
        //    }
        //}

        //#endregion

        #region OldMethod

        //protected virtual ICollection<EdataDictSubPath> CreateDictionaryEntriesForContentFiles(
        //    IEnumerable<EdataContentFile> contentFiles)
        //{
        //    //This alghoritm assumes that there are no two content files with the same paths.

        //    var remainingSplitPaths = contentFiles
        //        .OrderBy(file => file.Path)
        //        .Select(file => new ContentPathSplitInfo() { Path = file.Path })
        //        .ToList();

        //    var dictionaryEntries = new List<EdataDictSubPath>();

        //    while (remainingSplitPaths.Count > 0)
        //    {
        //        //powinny być tylko te pierwsze o wspólnym indeksie. //splitPathsWorkingSet
        //        var comparedPaths = remainingSplitPaths.ToList();
        //        int matchIndex = 0;

        //        //Ten warunek jest do dupy, bo na dobra sprawe nigdy petla nie kończy sie w wyniku jego spełnienia
        //        // a raczej w wyniku break.
        //        while (comparedPaths.Count > 0)
        //        {
        //            if (comparedPaths.Count == 1)
        //            {
        //                //
        //                var path = comparedPaths[0];

        //                var newFilePathEntry = new EdataDictFileSubPath();
        //                newFilePathEntry.SubPath = path.GetPathFromSplitIndex();

        //                AddEntryToDictionary(newFilePathEntry, path, dictionaryEntries);

        //                remainingSplitPaths.Remove(path);

        //                break;
        //            }

        //            int matchedPathsCount = 1;
        //            bool allPathsMatched = true;

        //            for (int i = 1; i < comparedPaths.Count; ++i)
        //            {
        //                var path1 = comparedPaths[i - 1];
        //                var path2 = comparedPaths[i];

        //                int index1 = matchIndex + path1.SplitIndex;
        //                int index2 = matchIndex + path2.SplitIndex;

        //                if (index1 == index2 &&
        //                    index1 < path1.Path.Length &&
        //                    index2 < path2.Path.Length &&
        //                    path1.Path[index1] == path2.Path[index2])
        //                {
        //                    matchedPathsCount++;
        //                }
        //                else
        //                {
        //                    // czy match end moze nie nastąpić? kluczowe pytanie, i co jesli tak i w jakiej sytuacji?
        //                    // update, w takiej sytuacji jak mamy obecnie to nie nastapi tylko gdy cała kolumna (wszysktie wpisy) w danym indeksie ma te same znaki
        //                    // czyl przechodzimy poprostu do nastepengo indesku z pełnym dopasowaniem.
        //                    allPathsMatched = false;
        //                    break;
        //                }
        //            }

        //            if (allPathsMatched)
        //            {
        //                matchIndex++;
        //            }
        //            else
        //            {
        //                if (matchIndex == 0)
        //                {
        //                    comparedPaths.RemoveRange(matchedPathsCount, comparedPaths.Count - matchedPathsCount);
        //                    matchIndex++;
        //                }
        //                else if (matchIndex > 0)
        //                {
        //                    var path = comparedPaths[0];

        //                    var newDirPathEntry = new EdataDictDirSubPath();
        //                    newDirPathEntry.SubPath = path.GetPathFromSplitIndex(length: matchIndex);

        //                    AddEntryToDictionary(newDirPathEntry, path, dictionaryEntries);

        //                    //czy tu powinno być += czy tylko przypisanie?
        //                    comparedPaths.ForEach(p => p.SplitIndex += matchIndex);

        //                    break;
        //                }
        //            }

        //        }
        //    }

        //    return dictionaryEntries;
        //}

        #endregion
    }
}
