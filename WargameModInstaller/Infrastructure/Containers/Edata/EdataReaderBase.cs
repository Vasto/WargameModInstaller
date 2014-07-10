using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Containers;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    public abstract class EdataReaderBase
    {
        public EdataReaderBase()
        {
            this.KnownContentFileTypes = CreateKnownContentFileTypes();
        }

        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<ContentFileType, byte[]> KnownContentFileTypes
        {
            get;
            private set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual Dictionary<ContentFileType, byte[]> CreateKnownContentFileTypes()
        {
            var knownTypes = new Dictionary<ContentFileType, byte[]>();
            knownTypes.Add(ContentFileType.Edata, ContentFileType.Edata.MagicBytes);
            knownTypes.Add(ContentFileType.Image, ContentFileType.Image.MagicBytes);
            knownTypes.Add(ContentFileType.Ndfbin, ContentFileType.Ndfbin.MagicBytes);
            knownTypes.Add(ContentFileType.Prxypcpc, ContentFileType.Prxypcpc.MagicBytes);
            knownTypes.Add(ContentFileType.Trad, ContentFileType.Trad.MagicBytes);
            knownTypes.Add(ContentFileType.Save, ContentFileType.Save.MagicBytes);

            return knownTypes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headerData"></param>
        /// <returns></returns>
        protected ContentFileType GetFileTypeFromHeaderData(byte[] headerData)
        {
            foreach (var knownType in KnownContentFileTypes)
            {
                byte[] headerMagic = new byte[knownType.Value.Length];
                Array.Copy(headerData, headerMagic, knownType.Value.Length);

                if (MiscUtilities.ComparerByteArrays(headerMagic, knownType.Value))
                {
                    return knownType.Key;
                }
            }

            return ContentFileType.Unknown;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual EdataHeader ReadHeader(Stream stream)
        {
            EdataHeader header;

            var buffer = new byte[Marshal.SizeOf(typeof(EdataHeader))];

            stream.Read(buffer, 0, buffer.Length);

            header = MiscUtilities.ByteArrayToStructure<EdataHeader>(buffer);

            return header;
        }

        /// <summary>
        /// Reads Edata version 2 dictionary entries
        /// </summary>
        /// <param name="source"></param>
        /// <param name="header"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual EdataDictionaryRootEntry ReadDcitionaryEntries(
            Stream source,
            uint dictOffset,
            uint dictLength)
        {
            EdataDictionaryRootEntry dictRoot = new EdataDictionaryRootEntry();
            EdataDictionaryDirEntry workingDir = null;

            source.Seek(dictOffset + dictRoot.Length, SeekOrigin.Begin);

            long dictEnd = dictOffset + dictLength;
            while (source.Position < dictEnd)
            {
                var buffer = new byte[4];
                source.Read(buffer, 0, 4);
                int entrysFirstFour = BitConverter.ToInt32(buffer, 0);

                source.Read(buffer, 0, 4);
                int entrysSecondFour = BitConverter.ToInt32(buffer, 0);

                bool isFileEntry = (entrysFirstFour == 0);
                if (isFileEntry)
                {
                    int entryLength = entrysSecondFour;

                    //source.Read(buffer, 0, 4);
                    //int relevanceLength = BitConverter.ToInt32(buffer, 0);

                    buffer = new byte[8];
                    source.Read(buffer, 0, buffer.Length);
                    long offset = BitConverter.ToInt64(buffer, 0);

                    source.Read(buffer, 0, buffer.Length);
                    long length = BitConverter.ToInt64(buffer, 0);

                    var checksum = new byte[16];
                    source.Read(checksum, 0, checksum.Length);

                    String pathPart = MiscUtilities.ReadString(source);
                    if (pathPart.Length % 2 == 0)
                    {
                        source.Seek(1, SeekOrigin.Current);
                    }

                    var newFile = new EdataDictionaryFileEntry(pathPart, entryLength, offset, length, checksum);
                    if (workingDir != null)
                    {
                        workingDir.AddFollowingEntry(newFile);

                        //Usuwamy tylko jeśli pojawia się wpis pliku oznaczony jako kończący ścieżkę.
                        if (newFile.IsEndingEntry())
                        {
                            EdataDictionaryDirEntry previousEntry = null;
                            do
                            {
                                previousEntry = workingDir;
                                workingDir = workingDir.PrecedingEntry as EdataDictionaryDirEntry;
                            }
                            while (workingDir != null && previousEntry.IsEndingEntry());
                        }
                    }
                    else
                    {
                        dictRoot.AddFollowingEntry(newFile);
                    }
                }
                else //isDirEntry
                {
                    int entryLength = entrysFirstFour;
                    int relevanceLength = entrysSecondFour;

                    //source.Read(buffer, 0, 4);
                    //int relevanceLength = BitConverter.ToInt32(buffer, 0);

                    String pathPart = MiscUtilities.ReadString(source);
                    if (pathPart.Length % 2 == 0)
                    {
                        source.Seek(1, SeekOrigin.Current);
                    }

                    var newDir = new EdataDictionaryDirEntry(pathPart, entryLength, relevanceLength);
                    if (workingDir != null)
                    {
                        workingDir.AddFollowingEntry(newDir);
                    }
                    else
                    {
                        dictRoot.AddFollowingEntry(newDir);
                    }
                    workingDir = newDir;
                }
            }

            return dictRoot;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dictionaryRoot"></param>
        /// <param name="contentFilesSectionOffset"></param>
        /// <returns></returns>
        protected virtual IEnumerable<EdataContentFile> TranslateDictionaryEntriesToContentFiles(
            Stream source,
            uint contentFilesSectionOffset,
            EdataDictionaryRootEntry dictionaryRoot)
        {
            var contentFiles = new List<EdataContentFile>();

            var fileEntries = dictionaryRoot.SelectEntriesOfType<EdataDictionaryFileEntry>();
            foreach (var entry in fileEntries)
            {
                var newCotnetFile = new EdataContentFile();
                newCotnetFile.Offset = entry.FileOffset;
                newCotnetFile.TotalOffset = entry.FileOffset + contentFilesSectionOffset;
                newCotnetFile.Length = entry.FileLength;
                newCotnetFile.Checksum = entry.FileChecksum;
                newCotnetFile.Path = entry.FullPath;

                //To można by stąd wydzielić
                ResolveContentFileType(source, newCotnetFile, contentFilesSectionOffset);

                contentFiles.Add(newCotnetFile);
            }

            return contentFiles;
        }

        private void ResolveContentFileType(
            Stream source,
            EdataContentFile file,
            uint contentFilesSectionOffset)
        {
            source.Seek(file.Offset + contentFilesSectionOffset, SeekOrigin.Begin);

            var headerBuffer = new byte[12];
            source.Read(headerBuffer, 0, headerBuffer.Length);

            file.FileType = GetFileTypeFromHeaderData(headerBuffer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="contentFiles"></param>
        protected void LoadContentFiles(Stream source, IEnumerable<EdataContentFile> contentFiles)
        {
            foreach (var file in contentFiles)
            {
                file.Content = ReadContent(source, file.TotalOffset, file.Length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        protected byte[] ReadContent(Stream stream, long offset, long size)
        {
            byte[] contentBuffer = new byte[size];

            stream.Seek(offset, SeekOrigin.Begin);
            stream.Read(contentBuffer, 0, contentBuffer.Length);

            return contentBuffer;
        }

        #region Helpers

        //protected void WriteContentFiles(String path, IEnumerable<EdataContentFile> files)
        //{
        //    var paths = files
        //        .Select(f => f.Path)
        //        .OrderBy(x => x, new EdataDictStringComparer())
        //        .ToList();

        //    using (var stream = File.CreateText(path))
        //    {
        //        foreach (var p in paths)
        //        {
        //            stream.WriteLine(p);
        //        }
        //    }
        //}

        //protected void ReadAndWriteDictionaryStats(
        //    Stream readStream,
        //    EdataHeader header,
        //    String filePath)
        //{
        //    readStream.Seek(header.DictOffset, SeekOrigin.Begin);
        //    readStream.Seek(10, SeekOrigin.Current);

        //    long dirEnd = header.DictOffset + header.DictLength;

        //    using (var writeStream = File.CreateText(filePath))
        //    {
        //        while (readStream.Position < dirEnd)
        //        {
        //            var buffer = new byte[4];
        //            readStream.Read(buffer, 0, 4);
        //            int fileGroupId = BitConverter.ToInt32(buffer, 0);

        //            if (fileGroupId == 0)
        //            {
        //                readStream.Read(buffer, 0, 4);
        //                int entrySize = BitConverter.ToInt32(buffer, 0);

        //                //skip ofsset 8, size 8, checsum 16
        //                readStream.Seek(32, SeekOrigin.Current);

        //                String name = MiscUtilities.ReadString(readStream);

        //                if (name.Length % 2 == 0)
        //                {
        //                    readStream.Seek(1, SeekOrigin.Current);
        //                }

        //                writeStream.WriteLine(String.Format("File, {0}, {1}, ", name, entrySize));
        //            }
        //            else
        //            {
        //                int entrySize = fileGroupId;

        //                readStream.Read(buffer, 0, 4);
        //                int relevance = BitConverter.ToInt32(buffer, 0);

        //                String name = MiscUtilities.ReadString(readStream);

        //                if (name.Length % 2 == 0)
        //                {
        //                    readStream.Seek(1, SeekOrigin.Current);
        //                }

        //                writeStream.WriteLine(String.Format("Dir, {0}, {1}, {2}", name, entrySize, relevance));
        //            }
        //        }
        //    }
        //} 
        #endregion

        #region Legacy

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <param name="header"></param>
        ///// <param name="loadContent"></param>
        ///// <returns>A Collection of the Files found in the Dictionary.</returns>
        ///// <remarks>
        ///// Credits to enohka for this method.
        ///// See more at: http://github.com/enohka/moddingSuite
        ///// "The only tricky part about that algorythm is that you have to skip one byte if the length of the File/Dir name PLUS nullbyte is an odd number."
        ///// </remarks>
        //protected virtual IEnumerable<EdataContentFile> ReadEdatV2Dictionary(
        //    Stream stream,
        //    EdataHeader header,
        //    bool loadContent,
        //    CancellationToken token)
        //{
        //    var files = new List<EdataContentFile>();
        //    var dirs = new List<EdataContentDirectory>();
        //    var endings = new List<long>();

        //    stream.Seek(header.DictOffset, SeekOrigin.Begin);

        //    long dirEnd = header.DictOffset + header.DictLength;
        //    uint id = 0;

        //    while (stream.Position < dirEnd)
        //    {
        //        //Cancel if requested;
        //        token.ThrowIfCancellationRequested();


        //        var buffer = new byte[4];
        //        stream.Read(buffer, 0, 4);
        //        int fileGroupId = BitConverter.ToInt32(buffer, 0);

        //        if (fileGroupId == 0)
        //        {
        //            var file = new EdataContentFile();
        //            stream.Read(buffer, 0, 4);
        //            file.FileEntrySize = BitConverter.ToInt32(buffer, 0);

        //            buffer = new byte[8];
        //            stream.Read(buffer, 0, buffer.Length);
        //            file.Offset = BitConverter.ToInt64(buffer, 0);
        //            file.TotalOffset = file.Offset + header.FileOffset;

        //            stream.Read(buffer, 0, buffer.Length);
        //            file.Size = BitConverter.ToInt64(buffer, 0);

        //            var checkSum = new byte[16];
        //            stream.Read(checkSum, 0, checkSum.Length);
        //            file.Checksum = checkSum;

        //            file.Name = MiscUtilities.ReadString(stream);
        //            file.Path = MergePath(dirs, file.Name);

        //            if (file.Name.Length % 2 == 0)
        //            {
        //                stream.Seek(1, SeekOrigin.Current);
        //            }

        //            //to Id służy do identyfikacji plików, oparte na kolejności odczytu, nie pochodzi z danych edata.
        //            file.Id = id;
        //            id++;

        //            ResolveContentFileType(stream, file, header);

        //            if (loadContent)
        //            {
        //                long currentStreamPosition = stream.Position;

        //                file.Content = ReadContent(stream, file.TotalOffset, file.Size);
        //                //file.Content = ReadContent(stream, header.FileOffset + file.Offset, file.Size);
        //                //file.Size = file.Content.Length;

        //                stream.Seek(currentStreamPosition, SeekOrigin.Begin);
        //            }

        //            files.Add(file);

        //            while (endings.Count > 0 && stream.Position == endings.Last())
        //            {
        //                dirs.Remove(dirs.Last());
        //                endings.Remove(endings.Last());
        //            }
        //        }
        //        else if (fileGroupId > 0)
        //        {
        //            var dir = new EdataContentDirectory();

        //            stream.Read(buffer, 0, 4);
        //            dir.FileEntrySize = BitConverter.ToInt32(buffer, 0);

        //            if (dir.FileEntrySize != 0)
        //            {
        //                endings.Add(dir.FileEntrySize + stream.Position - 8);
        //            }
        //            else if (endings.Count > 0)
        //            {
        //                endings.Add(endings.Last());
        //            }

        //            dir.Name = MiscUtilities.ReadString(stream);
        //            if (dir.Name.Length % 2 == 0)
        //            {
        //                stream.Seek(1, SeekOrigin.Current);
        //            }

        //            dirs.Add(dir);
        //        }
        //    }

        //    return files;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <param name="header"></param>
        ///// <param name="loadContent"></param>
        ///// <returns></returns>
        ///// <remarks>
        ///// Credits to enohka for this method.
        ///// See more at: http://github.com/enohka/moddingSuite
        ///// </remarks>
        //protected virtual IEnumerable<EdataContentFile> ReadEdatV1Dictionary(
        //    Stream stream,
        //    EdataHeader header,
        //    bool loadContent,
        //    CancellationToken token )
        //{
        //    var files = new List<EdataContentFile>();
        //    var dirs = new List<EdataContentDirectory>();
        //    var endings = new List<long>();

        //    stream.Seek(header.DictOffset, SeekOrigin.Begin);

        //    long dirEnd = header.DictOffset + header.DictLength;
        //    uint id = 0;

        //    while (stream.Position < dirEnd)
        //    {
        //        //Cancel if requested;
        //        token.ThrowIfCancellationRequested();


        //        var buffer = new byte[4];
        //        stream.Read(buffer, 0, 4);
        //        int fileGroupId = BitConverter.ToInt32(buffer, 0);

        //        if (fileGroupId == 0)
        //        {
        //            var file = new EdataContentFile();
        //            stream.Read(buffer, 0, 4);
        //            file.FileEntrySize = BitConverter.ToInt32(buffer, 0);

        //            //buffer = new byte[8];  - it's [4] now, so no need to change
        //            stream.Read(buffer, 0, 4);
        //            file.Offset = BitConverter.ToInt32(buffer, 0);
        //            file.TotalOffset = file.Offset + header.FileOffset;

        //            stream.Read(buffer, 0, 4);
        //            file.Size = BitConverter.ToInt32(buffer, 0);

        //            //var checkSum = new byte[16];
        //            //fileStream.Read(checkSum, 0, checkSum.Length);
        //            //file.Checksum = checkSum;
        //            stream.Seek(1, SeekOrigin.Current);  //instead, skip 1 byte - as in WEE DAT unpacker

        //            file.Name = MiscUtilities.ReadString(stream);
        //            file.Path = MergePath(dirs, file.Name);

        //            if ((file.Name.Length + 1) % 2 == 0)
        //            {
        //                stream.Seek(1, SeekOrigin.Current);
        //            }

        //            file.Id = id;
        //            id++;

        //            ResolveContentFileType(stream, file, header);

        //            if (loadContent)
        //            {
        //                long currentStreamPosition = stream.Position;

        //                file.Content = ReadContent(stream, file.TotalOffset, file.Size);
        //                file.Size = file.Content.Length; ////dodane

        //                stream.Seek(currentStreamPosition, SeekOrigin.Begin);
        //            }

        //            files.Add(file);

        //            while (endings.Count > 0 && stream.Position == endings.Last())
        //            {
        //                dirs.Remove(dirs.Last());
        //                endings.Remove(endings.Last());
        //            }
        //        }
        //        else if (fileGroupId > 0)
        //        {
        //            var dir = new EdataContentDirectory();

        //            stream.Read(buffer, 0, 4);
        //            dir.FileEntrySize = BitConverter.ToInt32(buffer, 0);

        //            if (dir.FileEntrySize != 0)
        //            {
        //                endings.Add(dir.FileEntrySize + stream.Position - 8);
        //            }
        //            else if (endings.Count > 0)
        //            {
        //                endings.Add(endings.Last());
        //            }

        //            dir.Name = MiscUtilities.ReadString(stream);
        //            if ((dir.Name.Length + 1) % 2 == 1)
        //            {
        //                stream.Seek(1, SeekOrigin.Current);
        //            }

        //            dirs.Add(dir);
        //        }
        //    }
        //    return files;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="stream"></param>
        ///// <param name="file"></param>
        ///// <remarks>
        ///// Credits to enohka for this method.
        ///// See more at: http://github.com/enohka/moddingSuite
        ///// </remarks>
        //protected virtual void ResolveContentFileType(Stream stream, EdataContentFile file, EdataHeader header)
        //{
        //    // save original offset
        //    long origOffset = stream.Position;

        //    stream.Seek(file.Offset + header.FileOffset, SeekOrigin.Begin);

        //    var headerBuffer = new byte[12];
        //    stream.Read(headerBuffer, 0, headerBuffer.Length);

        //    file.FileType = GetFileTypeFromHeaderData(headerBuffer);

        //    // set offset back to original
        //    stream.Seek(origOffset, SeekOrigin.Begin);
        //}

        ///// <summary>
        ///// Merges a filename in its dictionary tree.
        ///// </summary>
        ///// <param name="dirs"></param>
        ///// <param name="fileName"></param>
        ///// <returns>The valid Path inside the package.</returns>
        ///// <remarks>
        ///// Credits to enohka for this method.
        ///// See more at: http://github.com/enohka/moddingSuite
        ///// </remarks>
        //protected virtual String MergePath(IEnumerable<EdataContentDirectory> dirs, String fileName)
        //{
        //    var b = new StringBuilder();
        //    foreach (EdataContentDirectory dir in dirs)
        //    {
        //        b.Append(dir.Name);
        //    }
        //    b.Append(fileName);

        //    return b.ToString();
        //}

        ///// <summary>
        ///// Reads Edata version 2 dictionary into content files.
        ///// </summary>
        ///// <param name="source"></param>
        ///// <param name="header"></param>
        ///// <param name="token"></param>
        ///// <returns></returns>
        //protected virtual EdataDictionaryRootEntry ReadDcitionaryGood(
        //    Stream source,
        //    uint dictOffset,
        //    uint dictLength)
        //{
        //    var root = new EdataDictionaryRootEntry();
        //    var dirsStack = new Stack<EdataDictionaryDirEntry>();

        //    source.Seek(dictOffset + root.Length, SeekOrigin.Begin);

        //    long dictEnd = dictOffset + dictLength;
        //    while (source.Position < dictEnd)
        //    {
        //        var buffer = new byte[4];
        //        source.Read(buffer, 0, 4);
        //        int entrysFirstFour = BitConverter.ToInt32(buffer, 0);

        //        bool isFileEntry = (entrysFirstFour == 0);
        //        if (isFileEntry)
        //        {
        //            source.Read(buffer, 0, 4);
        //            int entryLenght = BitConverter.ToInt32(buffer, 0);

        //            buffer = new byte[8];
        //            source.Read(buffer, 0, buffer.Length);
        //            long offset = BitConverter.ToInt64(buffer, 0);

        //            source.Read(buffer, 0, buffer.Length);
        //            long length = BitConverter.ToInt64(buffer, 0);

        //            var checksum = new byte[16];
        //            source.Read(checksum, 0, checksum.Length);

        //            string pathPart = MiscUtilities.ReadString(source);
        //            if (pathPart.Length % 2 == 0)
        //            {
        //                source.Seek(1, SeekOrigin.Current);
        //            }

        //            var newEntry = new EdataDictionaryFileEntry(pathPart, (uint)entryLenght, offset, length, checksum);

        //            //WIP
        //            //if (dirsStack.Count > 0)
        //            //{
        //            //    var parentEntry = newEntry.IsPathEndingEntry() ?
        //            //        dirsStack.Pop() :
        //            //        dirsStack.Peek();
        //            //    parentEntry.AddFollowingEntry(newEntry);

        //            //    while (dirsStack.Count > 0 &&
        //            //        entryLenght == 0 &&
        //            //        parentEntry.IsPathEndingEntry())
        //            //    {
        //            //        parentEntry = dirsStack.Pop();
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    root.AddFollowingEntry(newEntry);
        //            //}

        //            //Verfied good,
        //            if (dirsStack.Count > 0)
        //            {
        //                var parentEntry = entryLenght == 0 ?
        //                    dirsStack.Pop() :
        //                    dirsStack.Peek();
        //                parentEntry.AddFollowingEntry(newEntry);

        //                while (dirsStack.Count > 0 &&
        //                    entryLenght == 0 &&
        //                    parentEntry.RelevanceLength == 0)
        //                {
        //                    parentEntry = dirsStack.Pop();
        //                }
        //            }
        //            else
        //            {
        //                root.AddFollowingEntry(newEntry);
        //            }
        //        }
        //        else //isDirEntry
        //        {
        //            //int entrySize = entrysFirstFour;

        //            source.Read(buffer, 0, 4);
        //            int relevance = BitConverter.ToInt32(buffer, 0);

        //            string pathPart = MiscUtilities.ReadString(source);
        //            if (pathPart.Length % 2 == 0)
        //            {
        //                source.Seek(1, SeekOrigin.Current);
        //            }

        //            var newEntry = new EdataDictionaryDirEntry(pathPart, (uint)relevance);
        //            if (dirsStack.Count > 0)
        //            {
        //                var parentEntry = dirsStack.Peek();
        //                parentEntry.AddFollowingEntry(newEntry);
        //            }
        //            else
        //            {
        //                root.AddFollowingEntry(newEntry);
        //            }
        //            dirsStack.Push(newEntry);

        //        }
        //        //dir entry ends when the second 4 bytes (relevance) are zero
        //        //file entry ends when the second 4 bytes (entrysize) are zero;
        //    }

        //    return root;
        //} 
        #endregion

    }

}
