using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.Infrastructure.Edata
{
    public class EdataBinWriter : EdataWriterBase, IEdataBinWriter
    {
        public byte[] Write(EdataFile edata)
        {
            return WriteContentInternal(edata);
        }

        public byte[] Write(EdataFile edata, CancellationToken token)
        {
            return WriteContentInternal(edata, token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edataFile"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        protected virtual byte[] WriteContentInternal(EdataFile edataFile, CancellationToken? token = null)
        {
            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            using (MemoryStream newEdataStream = new MemoryStream())
            {
                WriteHeader(newEdataStream, edataFile, token);

                WriteLoadedContent(newEdataStream, edataFile, token);

                WriteDictionary(newEdataStream, edataFile, token);

                return newEdataStream.ToArray();
            }
        }

    }

}
