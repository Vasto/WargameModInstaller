using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="header"></param>
        /// <param name="offset"></param>
        protected virtual void WriteHeader(Stream target, EdataHeader header, uint offset)
        {
            target.Seek(offset, SeekOrigin.Begin);

            byte[] rawHeader = MiscUtilities.StructToBytes(header);
            target.Write(rawHeader, 0, rawHeader.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dictionaryRoot"></param>
        /// <param name="dictOffset"></param>
        /// <returns></returns>
        protected virtual DictionaryWriteInfo WriteDictionary(
            Stream target,
            EdataDictionaryRootEntry dictionaryRoot,
            uint dictOffset)
        {
            var info = new DictionaryWriteInfo();

            target.Seek(dictOffset, SeekOrigin.Begin);

            //Need to use depth-traversal
            var entriesStack = new Stack<EdataDictionaryPathEntry>();
            entriesStack.Push(dictionaryRoot);

            while (entriesStack.Count > 0)
            {
                var entry = entriesStack.Pop();

                var entryBuffer = entry.ToBytes();
                target.Write(entryBuffer, 0, entryBuffer.Length);
                info.Length += (uint)entryBuffer.Length;

                foreach (var subEntry in entry.FollowingEntries.Reverse())
                {
                    entriesStack.Push(subEntry);
                }
            }

            var dictBuff = new byte[info.Length];
            target.Seek(dictOffset, SeekOrigin.Begin);
            target.Read(dictBuff, 0, dictBuff.Length);

            info.Checksum = MD5.Create().ComputeHash(dictBuff);

            return info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="contentFiles"></param>
        /// <param name="contentOffset"></param>
        /// <returns></returns>
        protected virtual ContentWriteInfo WriteLoadedContent(
            Stream target,
            IEnumerable<EdataContentFile> contentFiles,
            uint contentOffset,
            CancellationToken token)
        {
            ContentWriteInfo info = new ContentWriteInfo();

            //Dodane, spr:
            target.Seek(contentOffset, SeekOrigin.Begin);

            byte[] spaceBuffer = null;

            foreach (var file in contentFiles)
            {
                byte[] fileBuffer = file.Content;
                file.Checksum = MD5.Create().ComputeHash(fileBuffer);
                file.Length = file.Content.Length;
                file.Offset = target.Position - contentOffset;

                long spaceSize = GetSpaceSizeForFile(file);
                spaceBuffer = GetNewBufferIfNeeded(spaceBuffer, spaceSize);

                target.Write(fileBuffer, 0, fileBuffer.Length);
                target.Write(spaceBuffer, 0, (int)spaceSize);

                info.Length += (uint)fileBuffer.Length + (uint)spaceSize;

                token.ThrowIfCancellationRequested();
            }

            return info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="contentFiles"></param>
        /// <param name="contentOffset"></param>
        /// <returns></returns>
        protected virtual ContentWriteInfo WriteNotLoadedContent(
            Stream source,
            Stream target,
            IEnumerable<EdataContentFile> contentFiles,
            uint contentOffset,
            CancellationToken token)
        {
            ContentWriteInfo info = new ContentWriteInfo();

            //Dodane, spr:
            target.Seek(contentOffset, SeekOrigin.Begin);

            byte[] spaceBuffer = null;

            foreach (var file in contentFiles)
            {
                long oldOffset = file.Offset;
                file.Offset = target.Position - contentOffset;

                byte[] fileBuffer;

                if (file.IsContentLoaded)
                {
                    fileBuffer = file.Content;
                    file.Length = file.Content.Length;
                    file.Checksum = MD5.Create().ComputeHash(fileBuffer);
                }
                else
                {
                    fileBuffer = new byte[file.Length];
                    source.Seek(oldOffset + contentOffset, SeekOrigin.Begin);
                    source.Read(fileBuffer, 0, fileBuffer.Length);
                }

                long spaceSize = GetSpaceSizeForFile(file);
                spaceBuffer = GetNewBufferIfNeeded(spaceBuffer, spaceSize);

                target.Write(fileBuffer, 0, fileBuffer.Length);
                target.Write(spaceBuffer, 0, (int)spaceSize);

                info.Length += (uint)fileBuffer.Length + (uint)spaceSize;

                token.ThrowIfCancellationRequested();
            }

            return info;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="contentFiles"></param>
        protected virtual void WriteLoadedContentByReplace(
            Stream source,
            IEnumerable<EdataContentFile> contentFiles,
            CancellationToken token)
        {
            //Nie zmienia długości contentu

            byte[] spaceBuffer = null;

            foreach (var contentFile in contentFiles)
            {
                if (contentFile.IsContentLoaded)
                {
                    long orginalContentSize = contentFile.Length;

                    byte[] fileBuffer = contentFile.Content;
                    contentFile.Length = fileBuffer.Length;
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

                token.ThrowIfCancellationRequested();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        protected virtual void WritePadding(Stream target, uint offset, uint length)
        {
            byte[] buffer = new byte[length];

            target.Seek(offset, SeekOrigin.Begin);
            target.Write(buffer, 0, buffer.Length);
        }

        /// <summary>
        /// Assigns a dictionary data of the content files to the corresponding dictionary entries.
        /// </summary>
        /// <param name="contentFiles"></param>
        /// <param name="dictionaryRoot"></param>
        protected virtual void AssignContentFilesInfoToDictEntries(
            IEnumerable<EdataContentFile> contentFiles,
            EdataDictionaryRootEntry dictionaryRoot)
        {
            foreach (var file in contentFiles)
            {
                var matchingEntry = dictionaryRoot.SelectEntryByPath(file.Path);
                EdataDictionaryFileEntry fileEntry = matchingEntry as EdataDictionaryFileEntry;
                if (fileEntry != null)
                {
                    fileEntry.FileOffset = file.Offset;
                    fileEntry.FileLength = file.Length;
                    fileEntry.FileChecksum = file.Checksum;
                }
            }
        }

        /// <summary>
        /// Creates a hierarchy of the dictionary content entries based on the provided collection of content files.
        /// </summary>
        /// <param name="contentFiles"></param>
        /// <returns></returns>
        /// <remarks>This metohd requires that there are no two content files with the same paths.</remarks>
        protected virtual EdataDictionaryRootEntry CreateDictionaryEntries(
            IEnumerable<EdataContentFile> contentFiles)
        {
            List<ContentPathSplitInfo> pathsToSplit = contentFiles
                .OrderBy(file => file.Path, new EdataDictStringComparer())
                .Select(file => new ContentPathSplitInfo() { Path = file.Path })
                .ToList();

            var dictionaryRoot = new EdataDictionaryRootEntry();

            //Wide match can't be picked over a long match.
            while (pathsToSplit.Count > 0)
            {
                List<ContentPathSplitInfo> pathsToCompare = TakePathsToCompare(pathsToSplit);
                if (pathsToCompare.Count == 1)
                {
                    var newEntry = new EdataDictionaryFileEntry(pathsToCompare[0].GetPathFromSplitIndex());
                    AddEntryToDictionary(newEntry, pathsToCompare[0], dictionaryRoot);

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
                            var newEntry = new EdataDictionaryDirEntry(pathsToCompare[0].GetPathFromSplitIndex(length: matchIndex));
                            AddEntryToDictionary(newEntry, pathsToCompare[0], dictionaryRoot);

                            //czy tu powinno być += czy tylko przypisanie?
                            pathsToCompare.ForEach(x => x.SplitIndex += matchIndex);

                            break;
                        }
                    }
                }
            }

            return dictionaryRoot;
        }

        /// <summary>
        /// Selects a list of paths for comparison from an another list of paths. 
        /// Paths which can be compared have an equal SplitIndex value, and start with the same first character.
        /// </summary>
        /// <param name="pathsToSplit"></param>
        /// <returns></returns>
        private List<ContentPathSplitInfo> TakePathsToCompare(IList<ContentPathSplitInfo> pathsToSplit)
        {
            var pathsToCompare = new List<ContentPathSplitInfo>();

            var orgItem = pathsToSplit[0];
            pathsToCompare.Add(orgItem);

            //Zastąpić algorytmem porównującym z dzielenie obszaru porównywania. 
            //Czyli pierwsze porówniaie indeks 0 i ostatni jesli nie pasują to zero i pół itd, az do znalezienie dopasowania.
            for (int i = 1; i < pathsToSplit.Count; ++i)
            {
                var currentItem = pathsToSplit[i];

                if (currentItem.SplitIndex == orgItem.SplitIndex &&
                    currentItem.GetCharAtSplitIndex() == orgItem.GetCharAtSplitIndex())
                {
                    pathsToCompare.Add(pathsToSplit[i]);
                }
                else
                {
                    break;
                }
            }

            return pathsToCompare;
        }

        /// <summary>
        /// Checks wheather paths in the given list, have the same character value at the given index.
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
            EdataDictionaryPathEntry entry,
            ContentPathSplitInfo entryPathSplitInfo,
            EdataDictionaryRootEntry dictionaryRoot)
        {
            if (entryPathSplitInfo.SplitIndex == 0)
            {
                dictionaryRoot.AddFollowingEntry(entry);
                //entry.PrecedingEntry = dictionaryRoot;
            }
            else
            {
                var precedingPath = entryPathSplitInfo.GetPathToSplitIndex();
                EdataDictionaryPathEntry precedingPathEntry = 
                    dictionaryRoot.SelectEntryByPath(precedingPath);

                var pp = precedingPathEntry as EdataDictionaryDirEntry;
                if (precedingPathEntry != null)
                {
                    pp.AddFollowingEntry(entry);
                    //entry.PrecedingEntry = pp;
                }
                else
                {
                    throw new Exception(String.Format(
                        "Cannot find a following precedding entry: {0}",
                        precedingPath));
                }
            }
        }

        protected virtual uint GetHeaderOffset()
        {
            return 0;
        }

        protected virtual uint GetDictionaryOffset()
        {
            return 1037;
        }

        protected virtual uint ComputeDictionaryLength(EdataDictionaryRootEntry dictionaryRoot)
        {
            uint dictHeaderSize = 10;
            uint totalSize = dictHeaderSize;

            var entriesQueue = new Queue<EdataDictionaryPathEntry>();
            entriesQueue.Enqueue(dictionaryRoot);

            while (entriesQueue.Count > 0)
            {
                var entry = entriesQueue.Dequeue();

                totalSize += (uint)entry.Length;

                foreach (var subEntry in entry.FollowingEntries)
                {
                    entriesQueue.Enqueue(subEntry);
                }
            }

            return totalSize;
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

        protected bool AnyContentFileExceedsAvailableSpace(EdataFile file)
        {
            var contentFiles = file.ContentFiles
                .OfType<EdataContentFile>()
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

        protected long GetSpaceSizeForFile(EdataContentFile file)
        {
            long contentSize = file.IsContentLoaded ? file.ContentSize : file.Length;

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
