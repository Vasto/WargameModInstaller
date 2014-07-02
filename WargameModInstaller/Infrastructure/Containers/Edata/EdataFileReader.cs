using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    //To do: do przeróbki odczytywanie słownika tak aby wykorzystywało nowe elementy modelu.

    public class EdataFileReader : EdataReaderBase, IEdataFileReader
    {
        //private String lastEdataFilePath;

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
            var lastEdataFilePath = edataFilePath;

            EdataHeader header;
            IEnumerable<EdataContentFile> contentFiles;

            using (FileStream stream = new FileStream(lastEdataFilePath, FileMode.Open))
            {
                header = ReadHeader(stream, token);
                if (header.Version == 1)
                {
                    //ReadAndWriteDictionaryStats(stream, header, "C:\\ZZ_3.dat.txt");
                    contentFiles = ReadEdatV1Dictionary(stream, header, loadContent, token);
                }
                else if (header.Version == 2)
                {
                    //ReadAndWriteDictionaryStats(stream, header, "C:\\ZZ_3.dat.txt");
                    contentFiles = ReadEdatV2Dictionary(stream, header, loadContent, token);
                }
                else
                {
                    throw new NotSupportedException(String.Format("Edata Version {0} is currently not supported", header.Version));
                }
            }

            EdataFile edataFile = new EdataFile(lastEdataFilePath, header, contentFiles);
            //Może to powinien przypiswyać plik edata...?
            foreach (var contentFile in edataFile.ContentFiles)
            {
                contentFile.Owner = edataFile;
            }

            //WriteContentFiles("C:\\cf_mod_sorted.txt", contentFiles);

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
                return ReadContent(stream, file.TotalOffset, file.Size);   
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadContent(EdataContentFile file)
        {
            file.Content = ReadContent(file);
            //file.Size = file.Content.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        public void LoadContent(IEnumerable<EdataContentFile> files)
        {
            foreach (var file in files)
            {
                LoadContent(file);
            }
        }


    }
}
