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
    /// <summary>
    /// 
    /// </summary>
    public abstract class EdataWriterBase
    {
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
            var reserveBuffer = new byte[200];
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

                //Zastanowić się nad tym, czy by nie uzupełniać róznicy miedzy starym a nowym plikiem pustymi danymi, 
                //tak jak robi to reserveBuffer, tylko o odpowiedniej wielkości - miało by to być w celu sprawdzenia czy rzekomy
                //tak naprawde nie wiadomo czym do końca spowodowany problem nie ładowania modeli w amory przestałby istnieć.
                target.Write(fileBuffer, 0, fileBuffer.Length);
                target.Write(reserveBuffer, 0, reserveBuffer.Length);

                filesContentLength += (uint)fileBuffer.Length + (uint)reserveBuffer.Length;
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
            var reserveBuffer = new byte[200];
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

                target.Write(fileBuffer, 0, fileBuffer.Length);
                target.Write(reserveBuffer, 0, reserveBuffer.Length);

                filesContentLength += (uint)fileBuffer.Length + (uint)reserveBuffer.Length;
            }

            target.Seek(0x25, SeekOrigin.Begin);
            target.Write(BitConverter.GetBytes(filesContentLength), 0, 4);
        }

        /// <remarks>
        /// Method based on enohka's code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected virtual void WriteDictionary(Stream target, EdataFile edataFile, CancellationToken? token = null)
        {
            var sourceEdataHeader = edataFile.Header;

            target.Seek(sourceEdataHeader.DictOffset, SeekOrigin.Begin);
            long dictEnd = sourceEdataHeader.DictOffset + sourceEdataHeader.DictLength;
            uint id = 0;

            //Odtworzenie słownika?
            while (target.Position < dictEnd)
            {
                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();


                var buffer = new byte[4];
                target.Read(buffer, 0, 4);
                int fileGroupId = BitConverter.ToInt32(buffer, 0);

                if (fileGroupId == 0)
                {
                    EdataContentFile curFile = edataFile.ContentFiles
                        .Single(x => x.Id == id);

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

    }

}
