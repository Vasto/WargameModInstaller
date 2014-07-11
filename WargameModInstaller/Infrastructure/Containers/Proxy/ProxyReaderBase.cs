using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Containers.Proxy;

namespace WargameModInstaller.Infrastructure.Containers.Proxy
{
    //Ogólnie obecna metoda odczytu wpisów i trnalsacji ich do obiketów ProxyContentFile, jest narażona na błędy
    //w przypadku nierównej ilości wpisów tablicowych plików i ścieżek. Wynika to z faktu, że ścieżki są parowane 
    //z wpsiami pliku tworząc obiekt ProxyContentFile. Niesparowane wpisy zostają utracone, bo przy zapisie nowe
    //wpisy są tworzone na podstawi obiektów ProxyContentFile.

    public abstract class ProxyReaderBase
    {
        protected virtual ProxyHeader ReadHeader(Stream source, uint offset = 0)
        {
            ProxyHeader header;

            var buffer = new byte[Marshal.SizeOf(typeof(ProxyHeader))];

            source.Seek(0, SeekOrigin.Begin);
            source.Read(buffer, 0, buffer.Length);

            header = MiscUtilities.ByteArrayToStructure<ProxyHeader>(buffer);

            return header;
        }

        protected virtual IEnumerable<ProxyFileTableEntry> ReadFileTableEntries(Stream source, uint offset, uint length)
        {
            var entries = new List<ProxyFileTableEntry>();

            source.Seek(offset, SeekOrigin.Begin);

            uint sectionEnd = offset + length;
            while (source.Position < sectionEnd)
            {
                byte[] buffer = new byte[8];
                source.Read(buffer, 0, buffer.Length);
                byte[] hash = buffer;

                buffer = new byte[4];
                source.Read(buffer, 0, buffer.Length);
                uint fileOffset = BitConverter.ToUInt32(buffer, 0);

                source.Read(buffer, 0, buffer.Length);
                uint fileLength = BitConverter.ToUInt32(buffer, 0);

                source.Read(buffer, 0, buffer.Length);
                uint unknown = BitConverter.ToUInt32(buffer, 0);

                source.Read(buffer, 0, buffer.Length);
                uint padding = BitConverter.ToUInt32(buffer, 0);

                entries.Add(new ProxyFileTableEntry()
                {
                    Hash = hash,
                    Offset = fileOffset,
                    Length = fileLength,
                    Unknown = unknown,
                    Padding = padding
                });
            }

            return entries;
        }

        protected virtual IEnumerable<ProxyPathTableEntry> ReadPathTableEntries(Stream source, uint offset, uint length)
        {
            var entries = new List<ProxyPathTableEntry>();

            source.Seek(offset, SeekOrigin.Begin);

            uint sectionEnd = offset + length;
            while (source.Position < sectionEnd)
            {
                var path = MiscUtilities.ReadString(source, false);

                int skipSize = (int)ProxyPathTableEntry.EntryLength - 8 - path.Length;
                source.Seek(skipSize, SeekOrigin.Current);

                byte[] hash = new byte[8];
                source.Read(hash, 0, hash.Length);

                entries.Add(new ProxyPathTableEntry()
                {
                    Path = path,
                    Hash = hash,
                });
            }

            return entries;
        }

        protected virtual IEnumerable<ProxyContentFile> CreateContentFiles(
            IEnumerable<ProxyFileTableEntry> fileTableEntries,
            IEnumerable<ProxyPathTableEntry> pathTableEntries)
        {
            var files = new List<ProxyContentFile>();

            var pathTableEntriesDict = pathTableEntries.ToDictionary(x =>
                x.Hash, new ByteArrayComparer());

            foreach (var entry in fileTableEntries)
            {
                var newFile = new ProxyContentFile();
                newFile.Hash = entry.Hash;
                newFile.Length = entry.Length;
                newFile.Offset = entry.Offset;
                newFile.Padding = entry.Padding;
                newFile.Unknown = entry.Unknown;
                newFile.Path = pathTableEntriesDict[newFile.Hash].Path;

                files.Add(newFile);
            }

            return files;
        }

        protected void SetTotalOffsetsForContentFiles(
            IEnumerable<ProxyContentFile> files,
            uint filesSectionOffset)
        {
            foreach (var file in files)
            {
                file.TotalOffset = file.Offset + filesSectionOffset;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="contentFiles"></param>
        protected void LoadContentFiles(Stream source, IEnumerable<ProxyContentFile> contentFiles)
        {
            foreach (var file in contentFiles)
            {
                var content = ReadContent(source, file.TotalOffset, file.Length);
                file.LoadOrginalContent(content);
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
    }
}
