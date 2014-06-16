using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Common.Extensions;
using System.IO;

namespace WargameModInstaller.Infrastructure.Edata
{
    /// <summary>
    /// The Edata binary writer, wthout the dictionary building feature.
    /// </summary>
    public class EdataLegacyBinWriter : EdataLegacyWriterBase, IEdataBinWriter
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

            using (MemoryStream edataStream = new MemoryStream())
            {
                WriteHeader(edataStream, edataFile);

                if (CanUseReplacementWrite(edataFile))
                {
                    //Wydaję się że w tym wypadku to nigdy nie będzie miało miejsca, bo załadoway content oryginale
                    //(któy w tym przypadku powinien być załadowany zawsze w całości) zawsze będzie równy max dostępnej przestrzeni replacement.
                    WriteLoadedContentByReplace(edataStream, edataFile, token);
                }
                else
                {
                    WriteLoadedContent(edataStream, edataFile, token);
                }

                WriteDictionary(edataStream, edataFile);

                return edataStream.ToArray();
            }
        }

    }

}
