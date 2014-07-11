using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers.Proxy;

namespace WargameModInstaller.Infrastructure.Containers.Proxy
{
    public class ProxyBinReader : ProxyReaderBase, IProxyBinReader
    {
        public ProxyFile Read(byte[] rawProxy, bool loadContent = true)
        {
            return Read(rawProxy, loadContent, CancellationToken.None);
        }

        public ProxyFile Read(byte[] rawProxy, bool loadContent, CancellationToken token)
        {
            //Cancel if requested;
            token.ThrowIfCancellationRequested();

            ProxyHeader header;
            IEnumerable<ProxyContentFile> contentFiles = new List<ProxyContentFile>();

            using (MemoryStream stream = new MemoryStream(rawProxy))
            {
                if (!CanReadHeaderFromBuffer(rawProxy))
                {
                    throw new ArgumentException("Cannot read ProxyPCPC header from the buffer," +
                        " because header size exceeds size of the buffer", "rawProxy");
                }

                header = ReadHeader(stream);

                if (header.Version != 8)
                {
                    throw new NotSupportedException(
                        String.Format("ProxyPCPC Version {0} is currently not supported", header.Version));
                }

                var fileTableEntries = ReadFileTableEntries(stream, header.FileTableOffset, header.FileTableLength);
                var pathTableEntries = ReadPathTableEntries(stream, header.PathTableOffset, header.PathTableLength);

                contentFiles = CreateContentFiles(fileTableEntries, pathTableEntries);

                SetTotalOffsetsForContentFiles(contentFiles, header.ContentOffset);

                if (loadContent)
                {
                    LoadContentFiles(stream, contentFiles);
                }
            }

            ProxyFile proxyFile = new ProxyFile(header, contentFiles);

            return proxyFile;
        }

        private bool CanReadHeaderFromBuffer(byte[] dataBuffer)
        {
            return Marshal.SizeOf(typeof(ProxyHeader)) < dataBuffer.Length;
        }


    }
}
