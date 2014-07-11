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
    public class ProxyFileWriter : ProxyWriterBase, IProxyFileWriter
    {
        public void Write(ProxyFile file)
        {
            Write(file, CancellationToken.None);
        }

        public void Write(ProxyFile file, CancellationToken token)
        {
            //Cancel if requested;
            token.ThrowIfCancellationRequested();

            var header = file.Header;
            var proxyContentFiles = file.ContentFiles.OfType<ProxyContentFile>();

            //For now replacement write is not supported...

            bool allContentFilesLoaded = proxyContentFiles.All(x => x.IsContentLoaded);
            if (allContentFilesLoaded)
            {
                using (var target = new FileStream(file.Path, FileMode.OpenOrCreate))
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
                }
            }
            else
            {
                //We are checking it here, since when the content is not loaded, the file have to exist
                //so it could be read for content.
                if (!File.Exists(file.Path))
                {
                    throw new ArgumentException(
                        String.Format("A following ProxyPCPC file: \"{0}\" doesn't exist", file.Path),
                        "edataFile");
                }

                String temporaryProxyPath = GetTemporaryPathInCurrentLocation(file.Path);

                //To avoid too many nested try catches.
                FileStream source = null;
                FileStream target = null;
                try
                {
                    source = new FileStream(file.Path, FileMode.Open);
                    target = new FileStream(temporaryProxyPath, FileMode.Create);

                    uint fileTableOffset = (uint)Marshal.SizeOf(typeof(ProxyHeader));
                    //Póki co może być obliczona z ilości proxyContentFiles, bo to z nich zostanę utworzone wpisy fileTableEntries
                    //Sytuacja by się zmieniła jeśli te wpisy byłby by przechowywane od momentu odczytu, anie odrzuacane i odtwarzane jak obecnie.
                    uint fileTableLength = (uint)proxyContentFiles.Count() * ProxyFileTableEntry.EntryLength;

                    uint contentOffset = fileTableOffset + fileTableLength;
                    var contentWriteInfo = WriteNotLoadedContentFiles(source, target, contentOffset, proxyContentFiles, token);

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
                }
                finally
                {
                    //Spr czy zostały już zwolnione...?
                    CloseStreams(source, target);

                    if (File.Exists(temporaryProxyPath))
                    {
                        File.Delete(temporaryProxyPath);
                    }
                }
            }
        }

        /// <summary>
        /// To przenieśc do jakiego utilities i pomyśleć nad nazwa
        /// </summary>
        /// <returns></returns>
        protected String GetTemporaryPathInCurrentLocation(String oldeEdataPath)
        {
            var oldEdataFileInfo = new FileInfo(oldeEdataPath);
            var temporaryEdataPath = Path.Combine(
                oldEdataFileInfo.DirectoryName,
                Path.GetFileNameWithoutExtension(oldEdataFileInfo.Name) + ".tmp");

            return temporaryEdataPath;
        }

        //Zmienione z private na protected na razie
        protected void CloseStreams(Stream source, Stream target)
        {
            if (target != null)
            {
                target.Close();
            }

            if (source != null)
            {
                source.Close();
            }
        }

    }
}
