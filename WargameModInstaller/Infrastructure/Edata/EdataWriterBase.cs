using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Infrastructure.Edata
{
    //To do: wyglada na to że trzeba pomyśleć nad tymi wolnymi przestrzeniami. Bo te 100 bajtów to tak naprawde zaden bufor w przypadku zmiany rozmiaru obrazka..
    //a trzeb mieć na uwadze, że kazdy kto juz przebudował plik .dat przy pomocy instalatora ma już te min 200 bajtów odstepu...
    //chyba trzeba wrpowadzić jakieś proporcjonalne rozmiary w zaelznosci od rozmiaru pliku, wiadomo ze mały plik nie wzrośnie wielec wiecej w stosunku do swego rzmiaru
    //ale trzeb też mieć ograniczenia Max, z drugiej strony nie wiadomo jak to wpłynie na gre (wyglada na to ze max przestrzeń domyślan w ZZ_3 to ok 8000B)
    
    //Przy takim buforze proporcjonalnym trzeba mieć na uwadze ze podanie mniejszej wartości niż określona przez MinBytes spowoduje niemożliwość 
    //użycia replaceContent kiedykolwiek (warunek spr czy można użyć).

    /// <summary>
    /// 
    /// </summary>
    public abstract class EdataWriterBase
    {
        protected long MaxBytesBetweenFiles
        {
            get { return 8192; }
        }

        protected long MinBytesBetweenFiles
        {
            get { return 256; }
        }

        /// <remarks>
        /// Method based on enohka's code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual void WriteHeader(Stream target, EdataFile edataFile, CancellationToken? token = null)
        {
            //var sourceEdataHeader = edataFile.Header;
            //var headerPart = new byte[sourceEdataHeader.FileOffset];
            //sourceEdata.Read(headerPart, 0, headerPart.Length);
            //newEdata.Write(headerPart, 0, headerPart.Length);

            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            var sourceEdataHeader = edataFile.Header;
            byte[] rawHeader = MiscUtilities.StructToBytes(sourceEdataHeader);
            target.Write(rawHeader, 0, rawHeader.Length);
            target.Write(edataFile.PostHeaderData, 0, edataFile.PostHeaderData.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="edataFile"></param>
        /// <remarks>
        /// Method based on enohka's code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual void WriteLoadedContent(Stream target, EdataFile edataFile, CancellationToken? token = null)
        {
            byte[] spaceBuffer = null;
            var sourceEdataHeader = edataFile.Header;

            uint filesContentLength = 0;

            foreach (EdataContentFile file in edataFile.ContentFiles)
            {
                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();


                long oldOffset = file.Offset;
                file.Offset = target.Position - sourceEdataHeader.FileOffset;

                byte[] fileBuffer;

                fileBuffer = file.Content;
                file.Size = file.Content.Length; // To przenieść do klasy tak aby było ustawiane przy zmianie contnetu
                file.Checksum = MD5.Create().ComputeHash(fileBuffer);

                long spaceSize = GetSpaceSizeForFile(file);
                spaceBuffer = GetNewBufferIfNeeded(spaceBuffer, spaceSize);

                target.Write(fileBuffer, 0, fileBuffer.Length);
                target.Write(spaceBuffer, 0, (int)spaceSize);

                filesContentLength += (uint)fileBuffer.Length + (uint)spaceSize;
            }

            target.Seek(0x25, SeekOrigin.Begin);
            target.Write(BitConverter.GetBytes(filesContentLength), 0, 4);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="source">Source stream, from which content files will be loaded.</param>
        /// <param name="target"></param>
        /// <param name="edataFile"></param>
        /// <remarks>
        /// Method based on enohka's code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual void WriteNotLoadedContent(Stream source, Stream target, EdataFile edataFile, CancellationToken? token = null)
        {
            byte[] spaceBuffer = null;
            var sourceEdataHeader = edataFile.Header;

            source.Seek(sourceEdataHeader.FileOffset, SeekOrigin.Begin);

            uint filesContentLength = 0;

            foreach (EdataContentFile file in edataFile.ContentFiles)
            {
                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();


                long oldOffset = file.Offset;
                file.Offset = target.Position - sourceEdataHeader.FileOffset;

                byte[] fileBuffer;

                if (file.IsContentLoaded)
                {
                    fileBuffer = file.Content;
                    file.Size = file.Content.Length; // To przenieść do klasy tak aby było ustawiane przy zmianie contnetu
                    file.Checksum = MD5.Create().ComputeHash(fileBuffer); //To przyszło z dołu
                }
                else
                {
                    fileBuffer = new byte[file.Size];
                    source.Seek(oldOffset + sourceEdataHeader.FileOffset, SeekOrigin.Begin);
                    source.Read(fileBuffer, 0, fileBuffer.Length);
                }

                long spaceSize = GetSpaceSizeForFile(file);
                spaceBuffer = GetNewBufferIfNeeded(spaceBuffer, spaceSize);

                target.Write(fileBuffer, 0, fileBuffer.Length);
                target.Write(spaceBuffer, 0, (int)spaceSize);

                filesContentLength += (uint)fileBuffer.Length + (uint)spaceSize;
            }

            target.Seek(0x25, SeekOrigin.Begin);
            target.Write(BitConverter.GetBytes(filesContentLength), 0, 4);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="edataFile"></param>
        /// <param name="token"></param>
        protected virtual void ReplaceLoadedContent(Stream source, EdataFile edataFile, CancellationToken? token = null)
        {
            byte[] spaceBuffer = null;

            var contentFiles = edataFile.ContentFiles.ToArray();
            for (int i = 0; i < contentFiles.Length; ++i)
            {
                token.ThrowIfCanceledAndNotNull();

                var currentFile = contentFiles[i];
                if (!currentFile.IsContentLoaded)
                {
                    continue;
                }

                long orginalContentSize = currentFile.Size;

                byte[] fileBuffer = currentFile.Content;
                currentFile.Size = fileBuffer.Length;
                currentFile.Checksum = MD5.Create().ComputeHash(fileBuffer);

                source.Seek(currentFile.TotalOffset, SeekOrigin.Begin);
                source.Write(fileBuffer, 0, fileBuffer.Length);

                long contentSizeDffierence = orginalContentSize - currentFile.ContentSize;
                if (contentSizeDffierence > 0)
                {
                    spaceBuffer = GetNewBufferIfNeeded(spaceBuffer, contentSizeDffierence);
                    source.Write(spaceBuffer, 0, (int)contentSizeDffierence);
                }

                //Overwriting whole space, up to the next file:
                //var nextFile = (i + 1 < contentFiles.Length) ? contentFiles[i + 1] : null;
                //long fileSectionLength = edataFile.Header.FileOffset + edataFile.Header.FileLenght;
                //long spaceBetweenFiles = ((nextFile != null) ? nextFile.TotalOffset : fileSectionLength) - 
                //    (currentFile.TotalOffset + currentFile.ContentSize);

                //spaceBuffer = (spaceBuffer == null) ?
                //    new byte[spaceBetweenFiles] : ((spaceBetweenFiles > spaceBuffer.Length) ?
                //    new byte[spaceBetweenFiles] : spaceBuffer);

                //source.Write(spaceBuffer, 0, (int)spaceBetweenFiles);
            }
        }

        /// <remarks>
        /// Method based on enohka's code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual void WriteDictionary(Stream target, EdataFile edataFile, CancellationToken? token = null)
        {
            var sourceEdataHeader = edataFile.Header;
            var contentFilesDict = edataFile.ContentFiles.ToDictionary(x => x.Id);

            target.Seek(sourceEdataHeader.DictOffset, SeekOrigin.Begin);
            long dictEnd = sourceEdataHeader.DictOffset + sourceEdataHeader.DictLength;
            uint id = 0;

            //Odtworzenie słownika
            while (target.Position < dictEnd)
            {
                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();


                var buffer = new byte[4];
                target.Read(buffer, 0, 4);
                int fileGroupId = BitConverter.ToInt32(buffer, 0);

                if (fileGroupId == 0)
                {
                    EdataContentFile curFile = contentFilesDict[id];

                    // FileEntrySize
                    target.Seek(4, SeekOrigin.Current);

                    buffer = BitConverter.GetBytes(curFile.Offset);
                    target.Write(buffer, 0, buffer.Length);

                    buffer = BitConverter.GetBytes(curFile.Size);
                    target.Write(buffer, 0, buffer.Length);

                    byte[] checkSum = curFile.Checksum;
                    target.Write(checkSum, 0, checkSum.Length);

                    string name = MiscUtilities.ReadString(target);

                    if ((name.Length + 1) % 2 == 1)
                    {
                        target.Seek(1, SeekOrigin.Current);
                    }

                    id++;
                }
                else if (fileGroupId > 0)
                {
                    target.Seek(4, SeekOrigin.Current);
                    string name = MiscUtilities.ReadString(target);

                    if ((name.Length + 1) % 2 == 1)
                    {
                        target.Seek(1, SeekOrigin.Current);
                    }
                }
            }

            target.Seek(sourceEdataHeader.DictOffset, SeekOrigin.Begin);
            var dirBuffer = new byte[sourceEdataHeader.DictLength];
            target.Read(dirBuffer, 0, dirBuffer.Length);

            //Overwriting checksum
            byte[] dirCheckSum = MD5.Create().ComputeHash(dirBuffer);
            target.Seek(0x31, SeekOrigin.Begin);
            target.Write(dirCheckSum, 0, dirCheckSum.Length);
        }

        protected long GetSpaceSizeForFile(EdataContentFile file)
        {
            long contentSize = file.IsContentLoaded ? file.ContentSize : file.Size;

            long contentSizeSupplTo16 = MathUtilities.SupplementTo(contentSize, 16);

            double spaceToSizeCoeff = 0.002;

            long spaceSize = (long)(contentSize * spaceToSizeCoeff);
            spaceSize = MathUtilities.RoundUpToMultiple(spaceSize, 16);
            spaceSize = MathUtilities.Clamp(spaceSize, MinBytesBetweenFiles, MaxBytesBetweenFiles);
            //If space size + contentSizeSupplement is bigger than Max, subtracting 16 is enough, bcos
            //contentSizeSupplement wont be never bigger than 16.
            spaceSize = (spaceSize + contentSizeSupplTo16 > MaxBytesBetweenFiles) ?
                spaceSize + contentSizeSupplTo16 - 16 :
                spaceSize + contentSizeSupplTo16;

            return spaceSize;
        }

        protected bool CanUseReplacementWrite(EdataFile file)
        {
            //chyba zbedne to sotrtowaie, ale dla pewności...
            var contentFiles = file.ContentFiles
                .OrderBy(cf => cf.TotalOffset)
                .ToArray();

            for(int i = 0; i < contentFiles.Length; ++i)
            {
                var currentFile = contentFiles[i];
                if (!currentFile.IsContentLoaded)
                {
                    continue;
                }

                var nextFile = i + 1 < contentFiles.Length ? contentFiles[i + 1] : null;
                if (nextFile != null)
                {
                    if ((currentFile.TotalOffset + currentFile.ContentSize + MinBytesBetweenFiles) >=
                        nextFile.TotalOffset)
                    {
                        return false;
                    }
                }
                else
                {
                    if ((currentFile.TotalOffset + currentFile.ContentSize + MinBytesBetweenFiles) >= 
                        (file.Header.FileOffset + file.Header.FileLenght))
                    {
                        return false;
                    }
                }                
            }

            return true;
        }

        private byte[] GetNewBufferIfNeeded(byte[] orginalBuffer, long newSize)
        {
            bool needNewBuffer = (orginalBuffer == null) || (newSize > orginalBuffer.Length);
            if (needNewBuffer)
            {
                return new byte[newSize];
            }
            else
            {
                return orginalBuffer;
            }
        }

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

        protected virtual ICollection<EdataDictSubPath> CreateDictionaryEntriesForContentFiles(IEnumerable<EdataContentFile> contentFiles)
        {
            //This alghoritm assumes that there are no two content files with the same paths.

            //Przenisc to sortowanie gdzies indziej, bo sie powtarza w innych metodach.
            var pathsToSplit = contentFiles
                .OrderBy(file => file.Path)
                .Select(file => new ContentPathSplitInfo() { Path = file.Path })
                .ToList();

            var dictionaryEntries = new List<EdataDictSubPath>();

            //Wide match can't be picked over a long match.
            while (pathsToSplit.Count > 0)
            {
                var pathsToCompare = GetPathsToCompare(pathsToSplit);
                if (pathsToCompare.Count == 1)
                {
                    var newEntry = new EdataDictFileSubPath();
                    newEntry.SubPath = pathsToCompare[0].GetPathFromSplitIndex();
                    AddEntryToDictionary(newEntry, pathsToCompare[0], dictionaryEntries);

                    pathsToSplit.Remove(pathsToCompare[0]);
                }
                else if (pathsToCompare.Count > 1)
                {
                    int matchIndex = 0;

                    while (true) //Zastanowić sie co z warunkiem kończoaczym, czy to break moze nie zaistnieć.
                    {
                        bool allPathsMatched = CheckIfAllPathsMatchAtIndex(pathsToCompare, matchIndex); ;
                        if (allPathsMatched)
                        {
                            matchIndex++;
                        }
                        else
                        {
                            var newEntry = new EdataDictDirSubPath();
                            newEntry.SubPath = pathsToCompare[0].GetPathFromSplitIndex(length: matchIndex);
                            AddEntryToDictionary(newEntry, pathsToCompare[0], dictionaryEntries);

                            //czy tu powinno być += czy tylko przypisanie?
                            pathsToCompare.ForEach(x => x.SplitIndex += matchIndex);

                            break;
                        }
                    }
                }
            }

            return dictionaryEntries;
        }

        /// <summary>
        /// Selects a list of paths to comparison from an another  list of paths. 
        /// Paths which can be compared have an equal SplitIndex value, and start with the same first character.
        /// </summary>
        /// <param name="remainingSplitPaths"></param>
        /// <returns></returns>
        private List<ContentPathSplitInfo> GetPathsToCompare(IList<ContentPathSplitInfo> remainingSplitPaths)
        {
            var pathsToCompare = new List<ContentPathSplitInfo>();

            var orgItem = remainingSplitPaths[0];
            pathsToCompare.Add(orgItem);

            //Zastąpić algorytmem porównującym z dzielenie obszaru porównywania. Czyli pierwsze porówniaie indeks 0 i ostatni.
            //jesli nie pasują to zero i pół itd, az do znalezienie dopasowania.
            for (int i = 1; i < remainingSplitPaths.Count; ++i)
            {
                var currentItem = remainingSplitPaths[i];

                if (currentItem.SplitIndex == orgItem.SplitIndex &&
                    currentItem.GetCharAtSplitIndex() == orgItem.GetCharAtSplitIndex())
                {
                    pathsToCompare.Add(remainingSplitPaths[i]);
                }
                else
                {
                    break;
                }
            }

            return pathsToCompare;
        }

        /// <summary>
        /// Checks wheather paths in the given list, have the same character value at given index.
        /// </summary>
        /// <param name="comparedPaths"></param>
        /// <param name="matchIndex"></param>
        /// <returns></returns>
        private bool CheckIfAllPathsMatchAtIndex(IList<ContentPathSplitInfo> comparedPaths, int matchIndex)
        {
            var orgItem = comparedPaths[0];
            int orgIndex = orgItem.SplitIndex + matchIndex;

            for (int i = 1; i < comparedPaths.Count; ++i)
            {
                var currentItem = comparedPaths[i];
                int currentIndex = currentItem.SplitIndex + matchIndex;

                if (!(currentIndex == orgIndex &&
                    currentIndex < currentItem.Path.Length &&
                    orgIndex < orgItem.Path.Length &&
                    currentItem.Path[currentIndex] == orgItem.Path[orgIndex]))
                {
                    return false;
                }
            }

            return true;
        }

        private void AddEntryToDictionary(
            EdataDictSubPath entry,
            ContentPathSplitInfo entryPathSplitInfo,
            List<EdataDictSubPath> dictionaryEntries)
        {
            //First one so, no need to search for a predecessor.
            if (dictionaryEntries.Count == 0)
            {
                dictionaryEntries.Add(entry);
                return;
            }

            var precedingPath = entryPathSplitInfo.GetPathToSplitIndex();
            EdataDictSubPath precedingPathEntry = null;
            foreach (var e in dictionaryEntries)
            {
                var matchingEntry = e.SelectEntryByPath(precedingPath);
                if (matchingEntry != null)
                {
                    precedingPathEntry = matchingEntry;
                    break;
                }
            }

            var pp = precedingPathEntry as EdataDictDirSubPath;
            if (precedingPathEntry != null)
            {
                pp.AddFollowingSubPath(entry);
                entry.PrecedingSubPath = pp;
            }
        }

        #region Nested class ContentPathSplitInfo

        protected class ContentPathSplitInfo 
        {
            public String Path { get; set; }
            public int SplitIndex { get; set; }

            public String GetPathToSplitIndex()
            {
                return Path.Substring(0, SplitIndex);
            }

            public String GetPathFromSplitIndex()
            {
                return Path.Substring(SplitIndex);
            }

            public String GetPathFromSplitIndex(int length)
            {
                return Path.Substring(SplitIndex, length);
            }

            public char GetCharAtSplitIndex()
            {
                return Path[SplitIndex];
            }

            public override String ToString()
            {
                return Path.ToString();
            }
        }

        #endregion //ContentPathSplitInfo

    }

}
