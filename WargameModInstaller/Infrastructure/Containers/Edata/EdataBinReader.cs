using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    public class EdataBinReader : EdataReaderBase, IEdataBinReader
    {
        public EdataFile Read(byte[] rawEdata, bool loadContent = true)
        {
            return Read(rawEdata, loadContent, CancellationToken.None);
        }

        public EdataFile Read(byte[] rawEdata, bool loadContent, CancellationToken token)
        {
            //Cancel if requested;
            token.ThrowIfCancellationRequested();

            EdataHeader header;
            IEnumerable<EdataContentFile> contentFiles;

            using (MemoryStream stream = new MemoryStream(rawEdata))
            {
                if (!CanReadHeaderFromBuffer(rawEdata))
                {
                    throw new ArgumentException("Cannot read header from the buffer," + 
                        " because header size exceeds size of the buffer", "rawEdata");
                }

                header = ReadHeader(stream);

                if(!CanReadDictionaryFromBuffer(rawEdata, header.DictOffset, header.DictLength))
                {
                    throw new ArgumentException("Cannot read dictionary from the buffer," +
                        " because dictionary size exceeds size of the buffer", "rawEdata");
                }

                if (header.Version != 2)
                {
                    throw new NotSupportedException(String.Format("Edata Version {0} is currently not supported", header.Version));
                }

                var dictRoot = ReadDcitionaryEntries(stream, header.DictOffset, header.DictLength);
                contentFiles = TranslateDictionaryEntriesToContentFiles(stream, header.FileOffset,  dictRoot);

                if (loadContent)
                {
                    LoadContentFiles(stream, contentFiles);
                }
            }

            EdataFile edataFile = new EdataFile(header, contentFiles);

            return edataFile;
        }

        private bool CanReadHeaderFromBuffer(byte[] dataBuffer)
        {
            return Marshal.SizeOf(typeof(EdataHeader)) < dataBuffer.Length;
        }

        private bool CanReadDictionaryFromBuffer(byte[] dataBuffer, uint dictOffset, uint dictLength)
        {
            uint dictEnd = dictOffset + dictLength;

            return dictEnd < dataBuffer.Length;
        }

    }

}
