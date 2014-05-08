using Ionic.Zlib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Utilities.Compression
{
    //To do: rozwa¿yæ inne biblitoeki pod k¹tem lepszej kompresji, tak aby zminimalizowaæ koniecznoœc przebudowy plików.

    public class ZlibCompressor : ICompressor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public byte[] Decompress(byte[] input)
        {
            using (var output = new MemoryStream())
            {
                using (var zipStream = new ZlibStream(output, CompressionMode.Decompress))
                {
                    using (var inputStream = new MemoryStream(input))
                    {
                        byte[] buffer = input.Length > 4096 ? new byte[4096] : new byte[input.Length];

                        int size = 1;

                        while (size > 0)
                        {
                            size = inputStream.Read(buffer, 0, buffer.Length);
                            zipStream.Write(buffer, 0, size);
                        }
                    }
                }
                return output.ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public byte[] Compress(byte[] input)
        {
            using (var sourceStream = new MemoryStream(input))
            {
                using (var compressed = new MemoryStream())
                {
                    using (var zipSteam = new ZlibStream(compressed, CompressionMode.Compress, CompressionLevel.Level9, true))
                    {
                        zipSteam.FlushMode = FlushType.Full;

                        sourceStream.CopyTo(zipSteam);

                        zipSteam.Flush();

                        return compressed.ToArray();
                    }
                }
            }
        }

    }
}
