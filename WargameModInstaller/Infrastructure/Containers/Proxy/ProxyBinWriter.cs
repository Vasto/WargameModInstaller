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
    public class ProxyBinWriter : ProxyWriterBase, IProxyBinWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="edataFile"></param>
        /// <returns></returns>
        public virtual byte[] Write(ProxyFile file)
        {
            return Write(file, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edataFile"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public virtual byte[] Write(ProxyFile file, CancellationToken token)
        {
            //Cancel if requested;
            token.ThrowIfCancellationRequested();

            var header = file.Header;
            var proxyContentFiles = file.ContentFiles.OfType<ProxyContentFile>();

            //This assuemes that all content files are loaded.

            using (MemoryStream target = new MemoryStream())
            {
                uint fileTableOffset = (uint)Marshal.SizeOf(typeof(ProxyHeader));
                //Póki co może być obliczona z ilości proxyContentFiles, bo to z nich zostanę utworzone wpisy fileTableEntries
                //Sytuacja by się zmieniła jeśli te wpisy byłby by przechowywane od momentu odczytu, anie odrzuacane i odtwarzane jak obecnie.
                uint fileTableLength = (uint)proxyContentFiles.Count() * ProxyFileTableEntry.EntryLength;

                uint contentOffset = fileTableOffset + fileTableLength;
                var contentWriteInfo = WriteLoadedContentFiles(target, contentOffset, proxyContentFiles, token);

                //We are writing fileTable entries after writing the content, because we need to write actual offsets and lengths of content files.
                var fileTableEntries = CreateFileTableEntries(proxyContentFiles);
                WriteFileTableEntries(target, (uint)fileTableOffset, fileTableEntries);

                uint pathTableOffset = contentOffset + contentWriteInfo.Length;
                uint pathTableLength = (uint)proxyContentFiles.Count() * ProxyPathTableEntry.EntryLength;
                var pathTableEntries = CreatePathTableEntries(proxyContentFiles);
                WritePathTableEntries(target, pathTableOffset, pathTableEntries);

                header.FileTableOffset = fileTableOffset;
                header.FileTableLength = fileTableLength;
                header.FileTableEntriesCount = (uint)fileTableEntries.Count();
                header.ContentOffset = contentOffset;
                header.ContentLength = contentWriteInfo.Length;
                header.PathTableOffset = pathTableOffset;
                header.PathTableLength = pathTableLength;
                header.PathTableEntriesCount = (uint)pathTableEntries.Count();

                WriteHeader(target, header);

                return target.ToArray();
            }
        }

    }
}
