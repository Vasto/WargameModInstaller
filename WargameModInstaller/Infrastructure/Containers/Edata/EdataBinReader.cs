using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
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
            //byte[] postHeaderData;
            IEnumerable<EdataContentFile> contentFiles;

            using (MemoryStream stream = new MemoryStream(rawEdata))
            {
                header = ReadHeader(stream, token);

                //postHeaderData = ReadPostHeaderData(stream, header);

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
                    throw new NotSupportedException(string.Format("Edata Version {0} is currently not supported", header.Version));
                }

                //LoadContentFiles(stream, contentFiles);
            }

            //EdataFile edataFile = new EdataFile(header, postHeaderData, contentFiles);
            EdataFile edataFile = new EdataFile(header, contentFiles);
            //Może to powinien przypiswyać plik edata...?
            //foreach (var contentFile in edataFile.ContentFiles)
            //{
            //    contentFile.Owner = edataFile;
            //}

            return edataFile;
        }

    }

}
