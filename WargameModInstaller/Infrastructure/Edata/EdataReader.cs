using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Utilities;

namespace WargameModInstaller.Infrastructure.Edata
{
    public class EdataReader : IEdataReader
    {
        public static readonly byte[] EdataMagic = { 0x65, 0x64, 0x61, 0x74 };

        private String lastEdataFilePath;

        public EdataReader()
        {

        }

        /// <summary>
        /// Odczytuje cały plik edata.
        /// </summary>
        /// <returns></returns>
        public EdataFile ReadAll(String edataFilePath, bool loadContent = false)
        {
            if (!File.Exists(edataFilePath))
            {
                throw new ArgumentException(String.Format("File '{0}' doesn't exist.", edataFilePath), "edataFilePath");
            }
            lastEdataFilePath = edataFilePath;

            EdataHeader header = ReadHeader();
            IEnumerable<EdataContentFile> contentFiles;
            if (header.Version == 1)
            {
                contentFiles = ReadEdatV1Dictionary(header, loadContent);
            }
            else if (header.Version == 2)
            {
                contentFiles = ReadEdatV2Dictionary(header, loadContent);
            }
            else
            {
                throw new NotSupportedException(string.Format("Edata Version {0} is currently not supported", header.Version));
            }

            EdataFile edataFile = new EdataFile(lastEdataFilePath, header, contentFiles);
            //Może to powinien przypiswyać plik edata...?
            foreach (var contentFile in edataFile.ContentFiles)
            {
                contentFile.Owner = edataFile;
            }
            return edataFile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadContent(EdataContentFile file)
        {
            if (file.Owner == null)
            {
                throw new ArgumentException(String.Format("'{0}' is not assigned to any Edata File Owner.", file), "file");
            }
            var contentOwenr = file.Owner;
            if (!File.Exists(contentOwenr.Path))
            {
                throw new IOException(String.Format("Edata content owner file '{0}' doesn't exist.", contentOwenr.Path));
            }

            using (FileStream fs = File.OpenRead(contentOwenr.Path))
            {
                file.Content = ReadContent(fs, file.TotalOffset, file.Size);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadContent(IEnumerable<EdataContentFile> files)
        {
            foreach (var file in files)
            {
                LoadContent(file);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual EdataHeader ReadHeader()
        {
            var header = new EdataHeader();
            using (FileStream fileStream = File.Open(lastEdataFilePath, FileMode.Open))
            {
                var buffer = new byte[4];

                fileStream.Read(buffer, 0, buffer.Length);
                if (!MiscUtilities.ComparerByteArrays(buffer, EdataMagic))
                {
                    throw new InvalidDataException("The file is not edata conform or edata magic is missing.");
                }

                fileStream.Read(buffer, 0, buffer.Length);
                header.Version = BitConverter.ToInt32(buffer, 0);

                if (header.Version == 1)
                {
                    buffer = new byte[16];
                    fileStream.Read(buffer, 0, buffer.Length);
                    header.Checksum = buffer;
                    buffer = new byte[4];
                }
                else if (header.Version == 2)
                {
                    // Checksum is not here in V2
                    fileStream.Seek(16, SeekOrigin.Current);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Edata Version {0} is currently not supported", header.Version));
                }

                fileStream.Seek(1, SeekOrigin.Current);


                fileStream.Read(buffer, 0, 4);
                header.DirOffset = BitConverter.ToInt32(buffer, 0);

                fileStream.Read(buffer, 0, 4);
                header.DirLengh = BitConverter.ToInt32(buffer, 0);

                fileStream.Read(buffer, 0, 4);
                header.FileOffset = BitConverter.ToInt32(buffer, 0);

                fileStream.Read(buffer, 0, 4);
                header.FileLengh = BitConverter.ToInt32(buffer, 0);

                if (header.Version == 2)
                {
                    fileStream.Seek(8, SeekOrigin.Current);
                    buffer = new byte[16];
                    fileStream.Read(buffer, 0, buffer.Length);
                    header.Checksum = buffer;
                }
            }

            return header;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>A Collection of the Files found in the Dictionary</returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// "The only tricky part about that algorythm is that you have to skip one byte if the length of the File/Dir name PLUS nullbyte is an odd number."
        /// </remarks>
        protected virtual IEnumerable<EdataContentFile> ReadEdatV2Dictionary(EdataHeader header, bool loadContent = false)
        {
            var files = new List<EdataContentFile>();
            var dirs = new List<EdataContentDirectory>();
            var endings = new List<long>();

            using (FileStream fileStream = File.Open(lastEdataFilePath, FileMode.Open))
            {
                fileStream.Seek(header.DirOffset, SeekOrigin.Begin);

                long dirEnd = header.DirOffset + header.DirLengh;
                uint id = 0;

                while (fileStream.Position < dirEnd)
                {
                    var buffer = new byte[4];
                    fileStream.Read(buffer, 0, 4);
                    int fileGroupId = BitConverter.ToInt32(buffer, 0);

                    if (fileGroupId == 0)
                    {
                        var file = new EdataContentFile();
                        fileStream.Read(buffer, 0, 4);
                        file.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                        buffer = new byte[8];
                        fileStream.Read(buffer, 0, buffer.Length);
                        file.Offset = BitConverter.ToInt64(buffer, 0);
                        file.TotalOffset = file.Offset + header.FileOffset;

                        fileStream.Read(buffer, 0, buffer.Length);
                        file.Size = BitConverter.ToInt64(buffer, 0);

                        var checkSum = new byte[16];
                        fileStream.Read(checkSum, 0, checkSum.Length);
                        file.Checksum = checkSum;

                        file.Name = MiscUtilities.ReadString(fileStream);
                        file.Path = MergePath(dirs, file.Name);

                        if (file.Name.Length % 2 == 0)
                        {
                            fileStream.Seek(1, SeekOrigin.Current);
                        }

                        //to Id służy do identyfikacji plików, oparte na kolejności odczytu, nie pochodzi z danych edata.
                        file.Id = id;
                        id++;

                        ResolveFileType(fileStream, file, header);

                        if (loadContent)
                        {
                            long currentStreamPosition = fileStream.Position;

                            file.Content = ReadContent(fileStream, header.FileOffset + file.Offset, file.Size);

                            fileStream.Seek(currentStreamPosition, SeekOrigin.Begin);
                        }

                        files.Add(file);

                        while (endings.Count > 0 && fileStream.Position == endings.Last())
                        {
                            dirs.Remove(dirs.Last());
                            endings.Remove(endings.Last());
                        }
                    }
                    else if (fileGroupId > 0)
                    {
                        var dir = new EdataContentDirectory();

                        fileStream.Read(buffer, 0, 4);
                        dir.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                        if (dir.FileEntrySize != 0)
                        {
                            endings.Add(dir.FileEntrySize + fileStream.Position - 8);
                        }
                        else if (endings.Count > 0)
                        {
                            endings.Add(endings.Last());
                        }

                        dir.Name = MiscUtilities.ReadString(fileStream);
                        if (dir.Name.Length % 2 == 0)
                        {
                            fileStream.Seek(1, SeekOrigin.Current);
                        }

                        dirs.Add(dir);
                    }
                }
            }

            return files;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual IEnumerable<EdataContentFile> ReadEdatV1Dictionary(EdataHeader header, bool loadContent = false)
        {
            var files = new List<EdataContentFile>();
            var dirs = new List<EdataContentDirectory>();
            var endings = new List<long>();

            using (FileStream fileStream = File.Open(lastEdataFilePath, FileMode.Open))
            {
                fileStream.Seek(header.DirOffset, SeekOrigin.Begin);

                long dirEnd = header.DirOffset + header.DirLengh;
                uint id = 0;

                while (fileStream.Position < dirEnd)
                {
                    var buffer = new byte[4];
                    fileStream.Read(buffer, 0, 4);
                    int fileGroupId = BitConverter.ToInt32(buffer, 0);

                    if (fileGroupId == 0)
                    {
                        var file = new EdataContentFile();
                        fileStream.Read(buffer, 0, 4);
                        file.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                        //buffer = new byte[8];  - it's [4] now, so no need to change
                        fileStream.Read(buffer, 0, 4);
                        file.Offset = BitConverter.ToInt32(buffer, 0);
                        file.TotalOffset = file.Offset + header.FileOffset;

                        fileStream.Read(buffer, 0, 4);
                        file.Size = BitConverter.ToInt32(buffer, 0);

                        //var checkSum = new byte[16];
                        //fileStream.Read(checkSum, 0, checkSum.Length);
                        //file.Checksum = checkSum;
                        fileStream.Seek(1, SeekOrigin.Current);  //instead, skip 1 byte - as in WEE DAT unpacker

                        file.Name = MiscUtilities.ReadString(fileStream);
                        file.Path = MergePath(dirs, file.Name);

                        if ((file.Name.Length + 1) % 2 == 0)
                        {
                            fileStream.Seek(1, SeekOrigin.Current);
                        }

                        file.Id = id;
                        id++;

                        ResolveFileType(fileStream, file, header);

                        if (loadContent)
                        {
                            long currentStreamPosition = fileStream.Position;

                            file.Content = ReadContent(fileStream, file.TotalOffset, file.Size);

                            fileStream.Seek(currentStreamPosition, SeekOrigin.Begin);
                        }

                        files.Add(file);

                        while (endings.Count > 0 && fileStream.Position == endings.Last())
                        {
                            dirs.Remove(dirs.Last());
                            endings.Remove(endings.Last());
                        }
                    }
                    else if (fileGroupId > 0)
                    {
                        var dir = new EdataContentDirectory();

                        fileStream.Read(buffer, 0, 4);
                        dir.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                        if (dir.FileEntrySize != 0)
                        {
                            endings.Add(dir.FileEntrySize + fileStream.Position - 8);
                        }
                        else if (endings.Count > 0)
                        {
                            endings.Add(endings.Last());
                        }

                        dir.Name = MiscUtilities.ReadString(fileStream);
                        if ((dir.Name.Length + 1) % 2 == 1)
                        {
                            fileStream.Seek(1, SeekOrigin.Current);
                        }

                        dirs.Add(dir);
                    }
                }
            }
            return files;
        }

        protected byte[] ReadContent(FileStream fs, long offset, long size)
        {
            byte[] contentBuffer = new byte[size];

            fs.Seek(offset, SeekOrigin.Begin);
            fs.Read(contentBuffer, 0, contentBuffer.Length);

            return contentBuffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="file"></param>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual void ResolveFileType(FileStream fs, EdataContentFile file, EdataHeader header)
        {
            // save original offset
            long origOffset = fs.Position;

            fs.Seek(file.Offset + header.FileOffset, SeekOrigin.Begin);

            var headerBuffer = new byte[12];
            fs.Read(headerBuffer, 0, headerBuffer.Length);

            file.FileType = GetFileTypeFromHeaderData(headerBuffer);

            // set offset back to original
            fs.Seek(origOffset, SeekOrigin.Begin);
        }

        /// <summary>
        /// Merges a filename in its dictionary tree.
        /// </summary>
        /// <param name="dirs"></param>
        /// <param name="fileName"></param>
        /// <returns>The valid Path inside the package.</returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual string MergePath(IEnumerable<EdataContentDirectory> dirs, string fileName)
        {
            var b = new StringBuilder();
            foreach (EdataContentDirectory dir in dirs)
            {
                b.Append(dir.Name);
            }
            b.Append(fileName);

            return b.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="headerData"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public static EdataContentFileType GetFileTypeFromHeaderData(byte[] headerData)
        {
            byte[] edataHeader = EdataMagic;
            byte[] ndfbinheader = { 0x45, 0x55, 0x47, 0x30, 0x00, 0x00, 0x00, 0x00, 0x43, 0x4E, 0x44, 0x46 };
            byte[] tradHeader = { 0x54, 0x52, 0x41, 0x44 };
            byte[] savHeader = { 0x53, 0x41, 0x56, 0x30, 0x00, 0x00, 0x00, 0x00 };
            byte[] tgvHeader = { 2 };

            var knownHeaders = new List<KeyValuePair<EdataContentFileType, byte[]>>();
            knownHeaders.Add(new KeyValuePair<EdataContentFileType, byte[]>(EdataContentFileType.Ndfbin, ndfbinheader));
            knownHeaders.Add(new KeyValuePair<EdataContentFileType, byte[]>(EdataContentFileType.Package, edataHeader));
            knownHeaders.Add(new KeyValuePair<EdataContentFileType, byte[]>(EdataContentFileType.Dictionary, tradHeader));
            knownHeaders.Add(new KeyValuePair<EdataContentFileType, byte[]>(EdataContentFileType.Save, savHeader));
            knownHeaders.Add(new KeyValuePair<EdataContentFileType, byte[]>(EdataContentFileType.Image, tgvHeader));

            foreach (var knownHeader in knownHeaders)
            {
                if (knownHeader.Value.Length < headerData.Length)
                {
                    headerData = headerData.Take(knownHeader.Value.Length).ToArray();
                }

                if (MiscUtilities.ComparerByteArrays(headerData, knownHeader.Value))
                {
                    return knownHeader.Key;
                }
            }

            return EdataContentFileType.Unknown;
        }



    }
}
