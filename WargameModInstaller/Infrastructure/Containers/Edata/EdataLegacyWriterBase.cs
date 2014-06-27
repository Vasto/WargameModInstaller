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
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
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
    public abstract class EdataLegacyWriterBase
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
        protected virtual void WriteHeader(Stream target, EdataFile edataFile)
        {
            //var sourceEdataHeader = edataFile.Header;
            //var headerPart = new byte[sourceEdataHeader.FileOffset];
            //sourceEdata.Read(headerPart, 0, headerPart.Length);
            //newEdata.Write(headerPart, 0, headerPart.Length);

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

                //long oldOffset = file.Offset;

                byte[] fileBuffer = file.Content;
                file.Checksum = MD5.Create().ComputeHash(fileBuffer);
                file.Size = file.Content.Length; 
                file.Offset = target.Position - sourceEdataHeader.FileOffset;

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
        protected virtual void WriteLoadedContentByReplace(Stream source, EdataFile edataFile, CancellationToken? token = null)
        {
            byte[] spaceBuffer = null;

            var contentFiles = edataFile.ContentFiles.OfType<EdataContentFile>();
            foreach (var contentFile in contentFiles)
            {
                token.ThrowIfCanceledAndNotNull();

                if (!contentFile.IsContentLoaded)
                {
                    continue;
                }

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
        protected virtual void WriteDictionary(Stream target, EdataFile edataFile)
        {
            var sourceEdataHeader = edataFile.Header;
            var contentFilesDict = edataFile
                .ContentFiles
                .OfType<EdataContentFile>()
                .ToDictionary(x => x.Id);

            target.Seek(sourceEdataHeader.DictOffset, SeekOrigin.Begin);
            long dictEnd = sourceEdataHeader.DictOffset + sourceEdataHeader.DictLength;
            uint id = 0;

            //Odtworzenie słownika
            while (target.Position < dictEnd)
            {
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
            var dictBuffer = new byte[sourceEdataHeader.DictLength];
            target.Read(dictBuffer, 0, dictBuffer.Length);

            //Overwriting checksum
            byte[] dictCheckSum = MD5.Create().ComputeHash(dictBuffer);
            target.Seek(0x31, SeekOrigin.Begin);
            target.Write(dictCheckSum, 0, dictCheckSum.Length);
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
                .OfType<EdataContentFile>()
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

    }

}
