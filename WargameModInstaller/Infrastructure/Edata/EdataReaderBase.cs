using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Infrastructure.Edata
{
    public abstract class EdataReaderBase
    {
        protected static readonly byte[] EdataMagic = { 0x65, 0x64, 0x61, 0x74 };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual EdataHeader ReadHeader(Stream stream, CancellationToken? token = null)
        {
            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            EdataHeader header;

            var buffer = new byte[Marshal.SizeOf(typeof(EdataHeader))];

            stream.Read(buffer, 0, buffer.Length);

            header = MiscUtilities.ByteArrayToStructure<EdataHeader>(buffer);

            if (header.Version > 2)
            {
                throw new NotSupportedException(string.Format("Edata version {0} not supported", header.Version));
            }

            return header;
        }

        protected virtual byte[] ReadPostHeaderData(Stream stream, EdataHeader header, CancellationToken? token = null)
        {
            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            int headerSize = Marshal.SizeOf(typeof(EdataHeader)); //Marshal.SizeOf(header);
            byte[] buffer = new byte[header.FileOffset - headerSize];

            stream.Seek(headerSize, SeekOrigin.Begin);
            stream.Read(buffer, 0, buffer.Length);

            return buffer;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="header"></param>
        /// <param name="loadContent"></param>
        /// <returns>A Collection of the Files found in the Dictionary.</returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// "The only tricky part about that algorythm is that you have to skip one byte if the length of the File/Dir name PLUS nullbyte is an odd number."
        /// </remarks>
        protected virtual IEnumerable<EdataContentFile> ReadEdatV2Dictionary(
            Stream stream,
            EdataHeader header,
            bool loadContent = false,
            CancellationToken? token = null)
        {
            var files = new List<EdataContentFile>();
            var dirs = new List<EdataContentDirectory>();
            var endings = new List<long>();

            stream.Seek(header.DictOffset, SeekOrigin.Begin);

            long dirEnd = header.DictOffset + header.DictLength;
            uint id = 0;

            while (stream.Position < dirEnd)
            {
                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();


                var buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                int fileGroupId = BitConverter.ToInt32(buffer, 0);

                if (fileGroupId == 0)
                {
                    var file = new EdataContentFile();
                    stream.Read(buffer, 0, 4);
                    file.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    buffer = new byte[8];
                    stream.Read(buffer, 0, buffer.Length);
                    file.Offset = BitConverter.ToInt64(buffer, 0);
                    file.TotalOffset = file.Offset + header.FileOffset;

                    stream.Read(buffer, 0, buffer.Length);
                    file.Size = BitConverter.ToInt64(buffer, 0);

                    var checkSum = new byte[16];
                    stream.Read(checkSum, 0, checkSum.Length);
                    file.Checksum = checkSum;

                    file.Name = MiscUtilities.ReadString(stream);
                    file.Path = MergePath(dirs, file.Name);

                    if (file.Name.Length % 2 == 0)
                    {
                        stream.Seek(1, SeekOrigin.Current);
                    }

                    //to Id służy do identyfikacji plików, oparte na kolejności odczytu, nie pochodzi z danych edata.
                    file.Id = id;
                    id++;

                    ResolveFileType(stream, file, header);

                    if (loadContent)
                    {
                        long currentStreamPosition = stream.Position;

                        file.Content = ReadContent(stream, file.TotalOffset, file.Size);
                        //file.Content = ReadContent(stream, header.FileOffset + file.Offset, file.Size);
                        //file.Size = file.Content.Length;

                        stream.Seek(currentStreamPosition, SeekOrigin.Begin);
                    }

                    files.Add(file);

                    while (endings.Count > 0 && stream.Position == endings.Last())
                    {
                        dirs.Remove(dirs.Last());
                        endings.Remove(endings.Last());
                    }
                }
                else if (fileGroupId > 0)
                {
                    var dir = new EdataContentDirectory();

                    stream.Read(buffer, 0, 4);
                    dir.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    if (dir.FileEntrySize != 0)
                    {
                        endings.Add(dir.FileEntrySize + stream.Position - 8);
                    }
                    else if (endings.Count > 0)
                    {
                        endings.Add(endings.Last());
                    }

                    dir.Name = MiscUtilities.ReadString(stream);
                    if (dir.Name.Length % 2 == 0)
                    {
                        stream.Seek(1, SeekOrigin.Current);
                    }

                    dirs.Add(dir);
                }
            }

            return files;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="header"></param>
        /// <param name="loadContent"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual IEnumerable<EdataContentFile> ReadEdatV1Dictionary(
            Stream stream,
            EdataHeader header,
            bool loadContent = false,
            CancellationToken? token = null)
        {
            var files = new List<EdataContentFile>();
            var dirs = new List<EdataContentDirectory>();
            var endings = new List<long>();

            stream.Seek(header.DictOffset, SeekOrigin.Begin);

            long dirEnd = header.DictOffset + header.DictLength;
            uint id = 0;

            while (stream.Position < dirEnd)
            {
                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();


                var buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                int fileGroupId = BitConverter.ToInt32(buffer, 0);

                if (fileGroupId == 0)
                {
                    var file = new EdataContentFile();
                    stream.Read(buffer, 0, 4);
                    file.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    //buffer = new byte[8];  - it's [4] now, so no need to change
                    stream.Read(buffer, 0, 4);
                    file.Offset = BitConverter.ToInt32(buffer, 0);
                    file.TotalOffset = file.Offset + header.FileOffset;

                    stream.Read(buffer, 0, 4);
                    file.Size = BitConverter.ToInt32(buffer, 0);

                    //var checkSum = new byte[16];
                    //fileStream.Read(checkSum, 0, checkSum.Length);
                    //file.Checksum = checkSum;
                    stream.Seek(1, SeekOrigin.Current);  //instead, skip 1 byte - as in WEE DAT unpacker

                    file.Name = MiscUtilities.ReadString(stream);
                    file.Path = MergePath(dirs, file.Name);

                    if ((file.Name.Length + 1) % 2 == 0)
                    {
                        stream.Seek(1, SeekOrigin.Current);
                    }

                    file.Id = id;
                    id++;

                    ResolveFileType(stream, file, header);

                    if (loadContent)
                    {
                        long currentStreamPosition = stream.Position;

                        file.Content = ReadContent(stream, file.TotalOffset, file.Size);
                        file.Size = file.Content.Length; ////dodane

                        stream.Seek(currentStreamPosition, SeekOrigin.Begin);
                    }

                    files.Add(file);

                    while (endings.Count > 0 && stream.Position == endings.Last())
                    {
                        dirs.Remove(dirs.Last());
                        endings.Remove(endings.Last());
                    }
                }
                else if (fileGroupId > 0)
                {
                    var dir = new EdataContentDirectory();

                    stream.Read(buffer, 0, 4);
                    dir.FileEntrySize = BitConverter.ToInt32(buffer, 0);

                    if (dir.FileEntrySize != 0)
                    {
                        endings.Add(dir.FileEntrySize + stream.Position - 8);
                    }
                    else if (endings.Count > 0)
                    {
                        endings.Add(endings.Last());
                    }

                    dir.Name = MiscUtilities.ReadString(stream);
                    if ((dir.Name.Length + 1) % 2 == 1)
                    {
                        stream.Seek(1, SeekOrigin.Current);
                    }

                    dirs.Add(dir);
                }
            }
            return files;
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
                file.Content = ReadContent(source, file.TotalOffset, file.Size);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="file"></param>
        /// <remarks>
        /// Credits to enohka for this method.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual void ResolveFileType(Stream stream, EdataContentFile file, EdataHeader header)
        {
            // save original offset
            long origOffset = stream.Position;

            stream.Seek(file.Offset + header.FileOffset, SeekOrigin.Begin);

            var headerBuffer = new byte[12];
            stream.Read(headerBuffer, 0, headerBuffer.Length);

            file.FileType = GetFileTypeFromHeaderData(headerBuffer);

            // set offset back to original
            stream.Seek(origOffset, SeekOrigin.Begin);
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
        protected virtual String MergePath(IEnumerable<EdataContentDirectory> dirs, string fileName)
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
