using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Infrastructure.Edata
{
    public class EdataBinReader : EdataReaderBase, IEdataBinReader
    {
        public EdataFile Read(byte[] rawEdata, bool loadContent = true)
        {
            return ReadInternal(rawEdata, loadContent);
        }

        public EdataFile Read(byte[] rawEdata, bool loadContent, CancellationToken token)
        {
            return ReadInternal(rawEdata, loadContent, token);
        }

        /// <remarks>
        /// Method based on enohka's code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected EdataFile ReadInternal(byte[] rawEdata, bool loadContent, CancellationToken? token = null)
        {
            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            EdataHeader header;
            byte[] postHeaderData;
            IEnumerable<EdataContentFile> contentFiles;

            using (MemoryStream stream = new MemoryStream(rawEdata))
            {
                header = ReadHeader(stream);

                postHeaderData = ReadPostHeaderData(stream, header);

                if (header.Version == 1)
                {
                    contentFiles = ReadEdatV1Dictionary(stream, header, loadContent);
                }
                else if (header.Version == 2)
                {
                    contentFiles = ReadEdatV2Dictionary(stream, header, loadContent);
                }
                else
                {
                    throw new NotSupportedException(string.Format("Edata Version {0} is currently not supported", header.Version));
                }

                //LoadContentFiles(stream, contentFiles);
            }

            EdataFile edataFile = new EdataFile(header, postHeaderData, contentFiles);
            //Może to powinien przypiswyać plik edata...?
            //foreach (var contentFile in edataFile.ContentFiles)
            //{
            //    contentFile.Owner = edataFile;
            //}

            return edataFile;
        }

    }

}
