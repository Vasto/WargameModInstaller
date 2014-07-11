using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers.Proxy;

namespace WargameModInstaller.Infrastructure.Containers.Proxy
{
    public class ProxyFileReader : ProxyReaderBase, IProxyFileReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loadContent"></param>
        /// <returns></returns>
        public ProxyFile Read(String path, bool loadContent = false)
        {
            return Read(path, loadContent, CancellationToken.None);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loadContent"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public ProxyFile Read(String path, bool loadContent, CancellationToken token)
        {
            //Cancel if requested;
            token.ThrowIfCancellationRequested();

            if (!File.Exists(path))
            {
                throw new ArgumentException(String.Format("File '{0}' doesn't exist.", path), "path");
            }

            ProxyHeader header;
            IEnumerable<ProxyContentFile> contentFiles = new List<ProxyContentFile>();

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                header = ReadHeader(stream);

                if (header.Version != 8)
                {
                    throw new NotSupportedException(
                        String.Format("ProxyPCPC Version {0} is currently not supported", header.Version));
                }

                header = ReadHeader(stream);
                var fileTableEntries = ReadFileTableEntries(stream, header.FileTableOffset, header.FileTableLength);
                var pathTableEntries = ReadPathTableEntries(stream, header.PathTableOffset, header.PathTableLength);

                contentFiles = CreateContentFiles(fileTableEntries, pathTableEntries);

                SetTotalOffsetsForContentFiles(contentFiles, header.ContentOffset);

                if (loadContent)
                {
                    LoadContentFiles(stream, contentFiles);
                }
            }

            ProxyFile proxyFile = new ProxyFile(path, header, contentFiles);

            return proxyFile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadContent(ProxyContentFile file)
        {
            LoadContent(new ProxyContentFile[] { file });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadContent(IEnumerable<ProxyContentFile> files)
        {
            var filesGroupedByOwner = files.GroupBy(x => x.Owner);

            foreach (var fileGroup in filesGroupedByOwner)
            {
                var owner = fileGroup.Key;
                if (owner == null)
                {
                    throw new ArgumentException(
                        "One of the ProxyPCPC content files is not assigned to any ProxyPCPC container file.");
                }

                using (FileStream stream = File.OpenRead(owner.Path))
                {
                    foreach (var cf in fileGroup)
                    {
                        var content = ReadContent(stream, cf.TotalOffset, cf.Length);
                        cf.LoadOrginalContent(content);
                    }
                }
            }
        }

    }
}
