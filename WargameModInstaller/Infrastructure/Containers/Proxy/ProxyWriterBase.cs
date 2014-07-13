using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Containers.Proxy;

namespace WargameModInstaller.Infrastructure.Containers.Proxy
{
    public abstract class ProxyWriterBase
    {
        protected virtual void WriteHeader(Stream target, ProxyHeader header, uint offset = 0)
        {
            target.Seek(offset, SeekOrigin.Begin);

            byte[] rawHeader = MiscUtilities.StructToBytes(header);
            target.Write(rawHeader, 0, rawHeader.Length);
        }

        protected virtual ContentWriteInfo WriteLoadedContentFiles(
            Stream target,
            uint contentOffset,
            IEnumerable<ProxyContentFile> contentFiles,
            CancellationToken token)
        {
            //ProxyPcPC doesn't use any spacing between content files

            ContentWriteInfo info = new ContentWriteInfo();

            target.Seek(contentOffset, SeekOrigin.Begin);

            //Przydała by się jakaś funkcja haszujaca która potrafi zmiksować w hasz stary offset i długość
            //tyle trzeba uważać bo np przy dodawani które jest przeminne moze dosjc do powtózeń jesli offset i dł by sie zamieniły miejscami.

            //ProxyPCPC shares content between the files if content is the same. 
            //Therefore we're not writing each content uniquely but adjust values of file's offset to point to the same content spot.
            var uniqueContentEntries = new Dictionary<String, ProxyContentFile>();

            foreach (var file in contentFiles)
            {
                uint originalFileLength = file.Length;
                uint originalFileOffset = file.Offset;

                var thisContentKey = originalFileLength.ToString() + originalFileOffset.ToString();

                bool canReferenceWrittenContent = uniqueContentEntries.ContainsKey(thisContentKey) && !file.IsCustomContentLoaded;
                if (canReferenceWrittenContent)
                {
                    var matchingWrittenContent = uniqueContentEntries[thisContentKey];
                    file.Length = matchingWrittenContent.Length;
                    file.Offset = matchingWrittenContent.Offset;
                    file.TotalOffset = file.Offset + contentOffset;
                }
                else
                {
                    byte[] fileBuffer = file.Content;
                    file.Length = (uint)file.Content.Length;
                    file.Offset = (uint)target.Position - contentOffset;
                    file.TotalOffset = file.Offset + contentOffset;

                    target.Write(fileBuffer, 0, fileBuffer.Length);

                    info.Length += (uint)fileBuffer.Length;

                    uniqueContentEntries.Add(thisContentKey, file);
                }

                token.ThrowIfCancellationRequested();
            }

            return info;
        }

        protected virtual ContentWriteInfo WriteNotLoadedContentFiles(
            Stream source,
            Stream target,
            uint contentOffset,
            IEnumerable<ProxyContentFile> contentFiles,
            CancellationToken token)
        {
            ContentWriteInfo info = new ContentWriteInfo();

            target.Seek(contentOffset, SeekOrigin.Begin);

            var writtenContentFiles = new Dictionary<String, ProxyContentFile>();

            foreach (var file in contentFiles)
            {
                uint originalFileLength = file.Length;
                uint originalFileOffset = file.Offset;
                uint originalTotalOffset = file.TotalOffset;

                var thisContentKey = originalFileLength.ToString() + originalFileOffset.ToString();

                bool canReferenceWrittenContent = writtenContentFiles.ContainsKey(thisContentKey) && !file.IsCustomContentLoaded;
                if (canReferenceWrittenContent)
                {
                    var matchingWrittenContent = writtenContentFiles[thisContentKey];
                    file.Length = matchingWrittenContent.Length;
                    file.Offset = matchingWrittenContent.Offset;
                    file.TotalOffset = file.Offset + contentOffset;
                }
                else
                {
                    file.Offset = (uint)target.Position - contentOffset;
                    file.TotalOffset = file.Offset + contentOffset;

                    byte[] fileBuffer;

                    if (file.IsContentLoaded)
                    {
                        fileBuffer = file.Content;
                        file.Length = (uint)file.Content.Length;
                    }
                    else
                    {
                        fileBuffer = new byte[file.Length];
                        source.Seek(originalTotalOffset, SeekOrigin.Begin);
                        source.Read(fileBuffer, 0, fileBuffer.Length);
                    }

                    target.Write(fileBuffer, 0, fileBuffer.Length);

                    info.Length += (uint)fileBuffer.Length;

                    writtenContentFiles.Add(thisContentKey, file);
                }

                token.ThrowIfCancellationRequested();
            }

            return info;
        }

        protected virtual IEnumerable<ProxyFileTableEntry> CreateFileTableEntries(
            IEnumerable<ProxyContentFile> contentFiles)
        {
            var entries = new List<ProxyFileTableEntry>();

            foreach (var file in contentFiles)
            {
                var newEntry = new ProxyFileTableEntry();
                newEntry.Hash = file.Hash;
                newEntry.Offset = file.Offset;
                newEntry.Length = file.Length;
                newEntry.Unknown = file.Unknown;
                newEntry.Padding = file.Padding;

                entries.Add(newEntry);
            }

            return entries;
        }

        protected virtual IEnumerable<ProxyPathTableEntry> CreatePathTableEntries(
            IEnumerable<ProxyContentFile> contentFiles)
        {
            var entries = new List<ProxyPathTableEntry>();

            foreach (var file in contentFiles)
            {
                var newEntry = new ProxyPathTableEntry();
                newEntry.Hash = file.Hash;
                newEntry.Path = file.Path;

                entries.Add(newEntry);
            }

            return entries;
        }

        protected virtual void WriteFileTableEntries(
            Stream target,
            uint fileTableOffset,
            IEnumerable<ProxyFileTableEntry> entries)
        {
            target.Seek(fileTableOffset, SeekOrigin.Begin);

            byte[] buffer;
            foreach (var entry in entries)
            {
                target.Write(entry.Hash, 0, entry.Hash.Length);

                buffer = BitConverter.GetBytes(entry.Offset);
                target.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(entry.Length);
                target.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(entry.Unknown);
                target.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(entry.Padding);
                target.Write(buffer, 0, buffer.Length);
            }
        }

        protected virtual void WritePathTableEntries(
            Stream target,
            uint pathTableOffset,
            IEnumerable<ProxyPathTableEntry> entries)
        {
            target.Seek(pathTableOffset, SeekOrigin.Begin);

            byte[] buffer;
            foreach (var entry in entries)
            {
                buffer = Encoding.ASCII.GetBytes(entry.Path);
                target.Write(buffer, 0, buffer.Length);

                int pathLen = buffer.Length;
                int hashLen = entry.Hash.Length;
                int fillerLen = (int)ProxyPathTableEntry.EntryLength - pathLen - hashLen;

                buffer = new byte[fillerLen];
                target.Write(buffer, 0, buffer.Length);

                target.Write(entry.Hash, 0, entry.Hash.Length);
            }
        }

        #region Nested classes

        protected class ContentWriteInfo
        {
            public uint Length { get; set; }
        }

        #endregion
    }
}
