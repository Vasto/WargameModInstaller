using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Infrastructure.Edata
{
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

        protected virtual void WriteHeader(Stream target, EdataHeader header, uint offset)
        {
            target.Seek(offset, SeekOrigin.Begin);

            byte[] rawHeader = WargameModInstaller.Common.Utilities.MiscUtilities.StructToBytes(header);
            target.Write(rawHeader, 0, rawHeader.Length);
        }

        protected virtual DictionaryWriteInfo WriteDictionary(
            Stream target,
            IEnumerable<EdataDictSubPath> entries,
            uint dictOffset)
        {
            var info = new DictionaryWriteInfo();

            //WriteDictHeader
            target.Seek(dictOffset, SeekOrigin.Begin);

            byte[] buffer = { 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            target.Write(buffer, 0, buffer.Length);
            info.Length += (uint)buffer.Length;

            foreach (var rootEntry in entries)
            {
                //Need to use depth-traversal
                var entriesStack = new Stack<EdataDictSubPath>();
                entriesStack.Push(rootEntry);

                while (entriesStack.Count > 0)
                {
                    var entry = entriesStack.Pop();

                    var entryBuffer = entry.ToBytes();
                    target.Write(entryBuffer, 0, entryBuffer.Length);
                    info.Length += (uint)entryBuffer.Length;

                    foreach (var subEntry in entry.FollowingSubPaths.Reverse())
                    {
                        entriesStack.Push(subEntry);
                    }
                }
            }

            var dictBuff = new byte[info.Length];
            target.Seek(dictOffset, SeekOrigin.Begin);
            target.Read(dictBuff, 0, dictBuff.Length);

            info.Checksum = MD5.Create().ComputeHash(dictBuff);

            return info;
        }

        protected virtual ContentWriteInfo WriteLoadedContent(
            Stream target,
            IEnumerable<EdataContentFile> contentFiles,
            uint contentOffset)
        {
            ContentWriteInfo info = new ContentWriteInfo();

            byte[] spaceBuffer = null;

            foreach (var file in contentFiles)
            {
                //long oldOffset = file.Offset;

                byte[] fileBuffer = file.Content;
                file.Checksum = MD5.Create().ComputeHash(fileBuffer);
                file.Size = file.Content.Length;
                file.Offset = target.Position - contentOffset;

                long spaceSize = GetSpaceSizeForFile(file);
                spaceBuffer = GetNewBufferIfNeeded(spaceBuffer, spaceSize);

                target.Write(fileBuffer, 0, fileBuffer.Length);
                target.Write(spaceBuffer, 0, (int)spaceSize);

                info.Length += (uint)fileBuffer.Length + (uint)spaceSize;
            }

            return info;
        }

        protected virtual ContentWriteInfo WriteNotLoadedContent(
            Stream source,
            Stream target,
            IEnumerable<EdataContentFile> contentFiles,
            uint contentOffset)
        {
            ContentWriteInfo info = new ContentWriteInfo();

            source.Seek(contentOffset, SeekOrigin.Begin);

            byte[] spaceBuffer = null;

            foreach (var file in contentFiles)
            {
                long oldOffset = file.Offset;
                file.Offset = target.Position - contentOffset;

                byte[] fileBuffer;

                if (file.IsContentLoaded)
                {
                    fileBuffer = file.Content;
                    file.Size = file.Content.Length;
                    file.Checksum = MD5.Create().ComputeHash(fileBuffer);
                }
                else
                {
                    fileBuffer = new byte[file.Size];
                    source.Seek(oldOffset + contentOffset, SeekOrigin.Begin);
                    source.Read(fileBuffer, 0, fileBuffer.Length);
                }

                long spaceSize = GetSpaceSizeForFile(file);
                spaceBuffer = GetNewBufferIfNeeded(spaceBuffer, spaceSize);

                target.Write(fileBuffer, 0, fileBuffer.Length);
                target.Write(spaceBuffer, 0, (int)spaceSize);

                info.Length += (uint)fileBuffer.Length + (uint)spaceSize;
            }

            return info;
        }

        //Nie zmienia długości contentu
        protected virtual void WriteLoadedContentByReplace(
            Stream source,
            IEnumerable<EdataContentFile> contentFiles)
        {
            byte[] spaceBuffer = null;

            foreach (var contentFile in contentFiles)
            {
                if (contentFile.IsContentLoaded)
                {
                    long orginalContentSize = contentFile.Size;

                    byte[] fileBuffer = contentFile.Content;
                    contentFile.Size = fileBuffer.Length;
                    contentFile.Checksum = MD5.Create().ComputeHash(fileBuffer);

                    source.Seek(contentFile.TotalOffset, SeekOrigin.Begin);
                    source.Write(fileBuffer, 0, fileBuffer.Length);

                    long contentSizeDffierence = orginalContentSize - contentFile.ContentSize;
                    if (contentSizeDffierence > 0)
                    {
                        spaceBuffer = GetNewBufferIfNeeded(spaceBuffer, contentSizeDffierence);
                        source.Write(spaceBuffer, 0, (int)contentSizeDffierence);
                    }
                }
            }
        }

        protected virtual void WritePadding(Stream target, uint offset, uint length)
        {
            byte[] buffer = new byte[length];

            target.Seek(offset, SeekOrigin.Begin);
            target.Write(buffer, 0, buffer.Length);
        }

        protected virtual void AssignContentFilesInfoToDictEntries(
            IEnumerable<EdataContentFile> contentFiles,
            IEnumerable<EdataDictSubPath> dictEntries)
        {
            foreach (var file in contentFiles)
            {
                EdataDictFileSubPath matchingFileEntry = null;
                foreach (var entry in dictEntries)
                {
                    matchingFileEntry = (entry.SelectEntryByPath(file.Path) as EdataDictFileSubPath);
                    if (matchingFileEntry != null)
                    {
                        break;
                    }
                }

                if (matchingFileEntry != null)
                {
                    matchingFileEntry.FileOffset = file.Offset;
                    matchingFileEntry.FileLength = file.Size;
                    matchingFileEntry.FileChecksum = file.Checksum;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contentFiles"></param>
        /// <returns></returns>
        protected virtual IEnumerable<EdataDictSubPath> CreateDictionaryEntriesOfContentFiles(IEnumerable<EdataContentFile> contentFiles)
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
                    var newEntry = new EdataDictFileSubPath(pathsToCompare[0].GetPathFromSplitIndex());
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
                            EdataDictDirSubPath newEntry = null;
                            if (String.IsNullOrEmpty(pathsToCompare[0].GetPathToSplitIndex()))
                            {
                                newEntry = new EdataDictRootDirSubPath(pathsToCompare[0].GetPathFromSplitIndex(length: matchIndex));
                            }
                            else
                            {
                                newEntry = new EdataDictDirSubPath(pathsToCompare[0].GetPathFromSplitIndex(length: matchIndex));
                            }
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
            //It's a first one so no need to search for a predecessor.
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

        protected virtual uint GetDictionaryLength(IEnumerable<EdataDictSubPath> entries)
        {
            uint dictHeaderSize = 10;
            uint totalSize = dictHeaderSize;

            foreach (var rootEntry in entries)
            {
                var entriesQueue = new Queue<EdataDictSubPath>();
                entriesQueue.Enqueue(rootEntry);

                while (entriesQueue.Count > 0)
                {
                    var entry = entriesQueue.Dequeue();

                    totalSize += entry.TotalLength;

                    foreach (var subEntry in entry.FollowingSubPaths)
                    {
                        entriesQueue.Enqueue(subEntry);
                    }
                }
            }

            return totalSize;
        }

        protected virtual uint GetHeaderOffset()
        {
            return 0;
        }

        protected virtual uint GetDictionaryOffset()
        {
            return 1037;
        }

        protected bool AnyContentFileExceedsAvailableSpace(EdataFile file)
        {
            var contentFiles = file.ContentFiles
                .OrderBy(cf => cf.TotalOffset)
                .ToArray();

            for (int i = 0; i < contentFiles.Length; ++i)
            {
                var currentFile = contentFiles[i];
                if (currentFile.IsContentLoaded)
                {
                    var nextFile = i + 1 < contentFiles.Length ? contentFiles[i + 1] : null;
                    if (nextFile != null)
                    {
                        if ((currentFile.TotalOffset + currentFile.ContentSize + MinBytesBetweenFiles) >=
                            nextFile.TotalOffset)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if ((currentFile.TotalOffset + currentFile.ContentSize + MinBytesBetweenFiles) >=
                            (file.Header.FileOffset + file.Header.FileLenght))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        protected virtual uint GetFileOffset(uint dictionaryEnd)
        {
            //Zdaje się że ta wrtość jest zapisana w nagłówku, obecnie pod nazwą Padding.
            uint offsetStep = 8192;
            uint fileOffset = 0;

            while (fileOffset <= dictionaryEnd)
            {
                fileOffset += offsetStep;
            }

            return fileOffset;
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

        protected byte[] GetNewBufferIfNeeded(byte[] orginalBuffer, long newSize)
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

        #region Nested classes

        protected class DictionaryWriteInfo
        {
            public byte[] Checksum { get; set; }
            public uint Length { get; set; }
        }

        protected class ContentWriteInfo
        {
            public uint Length { get; set; }
        }

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

        #endregion
    }
}
