using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Utilities.Compression;
using WargameModInstaller.Utilities.Image.TGV;

namespace WargameModInstaller.Infrastructure.Image
{
    public class TgvBinReader : ITgvBinReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawTgvData"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public virtual TgvImage Read(byte[] rawTgvData)
        {
            var tgvFile = new TgvImage();

            using (var ms = new MemoryStream(rawTgvData))
            {
                var buffer = new byte[4];

                ms.Read(buffer, 0, buffer.Length);
                tgvFile.Version = BitConverter.ToUInt32(buffer, 0);

                ms.Read(buffer, 0, buffer.Length);
                tgvFile.IsCompressed = BitConverter.ToInt32(buffer, 0) > 0;

                ms.Read(buffer, 0, buffer.Length);
                tgvFile.Width = BitConverter.ToUInt32(buffer, 0);
                ms.Read(buffer, 0, buffer.Length);
                tgvFile.Height = BitConverter.ToUInt32(buffer, 0);

                ms.Read(buffer, 0, buffer.Length);
                tgvFile.ImageHeight = BitConverter.ToUInt32(buffer, 0);
                ms.Read(buffer, 0, buffer.Length);
                tgvFile.ImageWidth = BitConverter.ToUInt32(buffer, 0);

                buffer = new byte[2];

                ms.Read(buffer, 0, buffer.Length);
                tgvFile.MipMapCount = BitConverter.ToUInt16(buffer, 0);

                ms.Read(buffer, 0, buffer.Length);
                ushort pixelFormatLen = BitConverter.ToUInt16(buffer, 0);

                buffer = new byte[pixelFormatLen];

                ms.Read(buffer, 0, buffer.Length);
                tgvFile.PixelFormatString = Encoding.ASCII.GetString(buffer);

                ms.Seek(MiscUtilities.RoundToNextDivBy4(pixelFormatLen) - pixelFormatLen, SeekOrigin.Current);

                buffer = new byte[16];
                ms.Read(buffer, 0, buffer.Length);
                tgvFile.SourceChecksum = (byte[])buffer.Clone();

                buffer = new byte[4];

                for (int i = 0; i < tgvFile.MipMapCount; i++)
                {
                    ms.Read(buffer, 0, buffer.Length);
                    uint offset = BitConverter.ToUInt32(buffer, 0);
                    tgvFile.Offsets.Add(offset);
                }

                for (int i = 0; i < tgvFile.MipMapCount; i++)
                {
                    ms.Read(buffer, 0, buffer.Length);
                    uint offset = BitConverter.ToUInt32(buffer, 0);
                    tgvFile.Sizes.Add(offset);
                }

                for (int i = 0; i < tgvFile.MipMapCount; i++)
                {
                    tgvFile.MipMaps.Add(ReadMipMap(i, rawTgvData, tgvFile));
                }
            }

            tgvFile.Format = TgvUtilities.GetPixelFormatFromTgv(tgvFile.PixelFormatString);

            return tgvFile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        protected TgvMipMap ReadMipMap(int id, byte[] rawTgvData, TgvImage tgvFile)
        {
            if (id > tgvFile.MipMapCount)
            {
                throw new ArgumentException("id");
            }

            var zipo = new byte[] { 0x5A, 0x49, 0x50, 0x4F };

            var mipMap = new TgvMipMap(tgvFile.Offsets[id], tgvFile.Sizes[id], 0);

            using (var ms = new MemoryStream(rawTgvData, (int)mipMap.Offset, (int)mipMap.Size))
            {
                var buffer = new byte[4];

                if (tgvFile.IsCompressed)
                {
                    ms.Read(buffer, 0, buffer.Length);
                    if (!MiscUtilities.ComparerByteArrays(buffer, zipo))
                    {
                        throw new InvalidDataException("Mipmap has to start with \"ZIPO\"!");
                    }

                    ms.Read(buffer, 0, buffer.Length);
                    mipMap.MipWidth = BitConverter.ToInt32(buffer, 0);
                }

                buffer = new byte[ms.Length - ms.Position];
                ms.Read(buffer, 0, buffer.Length);

                if (tgvFile.IsCompressed)
                {
                    ICompressor comp = new ZlibCompressor();
                    buffer = comp.Decompress(buffer);
                }

                mipMap.Content = buffer;

                return mipMap;
            }
        }

    }
}
