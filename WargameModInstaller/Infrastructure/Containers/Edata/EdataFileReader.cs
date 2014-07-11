using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    public class EdataFileReader : EdataReaderBase, IEdataFileReader
    {
        public EdataFile Read(String edataFilePath, bool loadContent = false)
        {
            return Read(edataFilePath, loadContent, CancellationToken.None);
        }

        public EdataFile Read(String edataFilePath, bool loadContent, CancellationToken token)
        {
            //Cancel if requested;
            token.ThrowIfCancellationRequested();

            if (!File.Exists(edataFilePath))
            {
                throw new ArgumentException(String.Format("File '{0}' doesn't exist.", edataFilePath), "edataFilePath");
            }

            EdataHeader header;
            IEnumerable<EdataContentFile> contentFiles;

            using (FileStream stream = new FileStream(edataFilePath, FileMode.Open))
            {
                header = ReadHeader(stream);

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

            EdataFile edataFile = new EdataFile(edataFilePath, header, contentFiles);

            return edataFile;
        }

        /// <summary>
        /// Reads content described by the given content file, but doesn't assing it to that Content File.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] ReadContent(EdataContentFile file)
        {
            if (file.Owner == null)
            {
                throw new ArgumentException(String.Format("'{0}' is not assigned to any Edata File Owner.", file), "file");
            }
            var contentOwenr = file.Owner;
            if (!File.Exists(contentOwenr.Path))
            {
                throw new IOException(String.Format("Edata content owner file '{0}' doesn't exist.", contentOwenr.Path));
            }

            using (FileStream stream = File.OpenRead(contentOwenr.Path))
            {
                return ReadContent(stream, file.TotalOffset, file.Length);   
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadContent(EdataContentFile file)
        {
            var content = ReadContent(file);
            file.LoadOrginalContent(content);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadContent(IEnumerable<EdataContentFile> files)
        {
            var filesGroupedByOwner = files.GroupBy(x => x.Owner);

            foreach (var fileGroup in filesGroupedByOwner)
            {
                var owner = fileGroup.Key;
                if (owner == null)
                {
                    throw new ArgumentException("One of the Edata content files is not assigned to any Edata container file.");
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
