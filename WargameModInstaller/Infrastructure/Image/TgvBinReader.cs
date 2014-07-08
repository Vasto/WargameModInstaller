using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Common.Utilities.Compression;
using WargameModInstaller.Common.Utilities.Image.TGV;

namespace WargameModInstaller.Infrastructure.Image
{
    /// <summary>
    /// A reader which can read a TGV image file from the raw bytes data.
    /// </summary>
    public class TgvBinReader : ITgvBinReader
    {
        protected static readonly byte[] ZIPO = new byte[] { 0x5A, 0x49, 0x50, 0x4F };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawTgvData"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public virtual TgvImage Read(byte[] rawTgvData, bool loadMipMaps = true)
        {
            var tgv = new TgvImage();

            using (var ms = new MemoryStream(rawTgvData))
            {
                var buffer = new byte[4];

                ms.Read(buffer, 0, buffer.Length);
                tgv.Version = BitConverter.ToUInt32(buffer, 0);

                ms.Read(buffer, 0, buffer.Length);
                tgv.IsCompressed = BitConverter.ToInt32(buffer, 0) > 0;

                ms.Read(buffer, 0, buffer.Length);
                tgv.Width = BitConverter.ToUInt32(buffer, 0);
                ms.Read(buffer, 0, buffer.Length);
                tgv.Height = BitConverter.ToUInt32(buffer, 0);

                ms.Read(buffer, 0, buffer.Length);
                tgv.ImageHeight = BitConverter.ToUInt32(buffer, 0);
                ms.Read(buffer, 0, buffer.Length);
                tgv.ImageWidth = BitConverter.ToUInt32(buffer, 0);

                buffer = new byte[2];

                ms.Read(buffer, 0, buffer.Length);
                tgv.MipMapCount = BitConverter.ToUInt16(buffer, 0);

                ms.Read(buffer, 0, buffer.Length);
                ushort pixelFormatLen = BitConverter.ToUInt16(buffer, 0);

                buffer = new byte[pixelFormatLen];

                ms.Read(buffer, 0, buffer.Length);
                tgv.PixelFormatString = Encoding.ASCII.GetString(buffer);

                ms.Seek(MathUtilities.RoundToNextDivBy4(pixelFormatLen) - pixelFormatLen, SeekOrigin.Current);

                buffer = new byte[16];
                ms.Read(buffer, 0, buffer.Length);
                tgv.SourceChecksum = (byte[])buffer.Clone();

                //Read MipMaps
                var mipMaps = CreateEmptyMipMaps(tgv.MipMapCount);

                ReadMipMapsOffsets(ms, mipMaps);

                ReadMipMapsLengths(ms, mipMaps);

                if (loadMipMaps)
                {
                    ReadMipMapsContent(ms, mipMaps, tgv.IsCompressed);
                }

                SetMipMapsDimensions(mipMaps, tgv.Width, tgv.Height);

                tgv.MipMaps = mipMaps;
            }

            tgv.Format = TgvUtilities.GetPixelFormatFromTgv(tgv.PixelFormatString);

            return tgv;
        }

        private IList<TgvMipMap> CreateEmptyMipMaps(int count)
        {
            var mipMaps = new List<TgvMipMap>();
            for (int i = 0; i < count; ++i)
            {
                mipMaps.Add(new TgvMipMap());
            }

            return mipMaps;
        }

        private void ReadMipMapsOffsets(Stream stream, IList<TgvMipMap> mipMaps)
        {
            var buffer = new byte[4];
            for (int i = 0; i < mipMaps.Count; i++)
            {
                stream.Read(buffer, 0, buffer.Length);
                uint offset = BitConverter.ToUInt32(buffer, 0);
                mipMaps[i].Offset = offset;
            }
        }

        private void ReadMipMapsLengths(Stream stream, IList<TgvMipMap> mipMaps)
        {
            var buffer = new byte[4];
            for (int i = 0; i < mipMaps.Count; i++)
            {
                stream.Read(buffer, 0, buffer.Length);
                uint length = BitConverter.ToUInt32(buffer, 0);
                mipMaps[i].Length = length;
            }
        }

        private void ReadMipMapsContent(Stream stream, IList<TgvMipMap> mipMaps, bool compressed)
        {
            for (int i = 0; i < mipMaps.Count; i++)
            {
                var buffer = new byte[4];

                stream.Seek(mipMaps[i].Offset, SeekOrigin.Begin);

                if (compressed)
                {
                    stream.Read(buffer, 0, buffer.Length);
                    if (!MiscUtilities.ComparerByteArrays(buffer, ZIPO))
                    {
                        throw new InvalidDataException("Mipmap has to start with \"ZIPO\"!");
                    }

                    stream.Read(buffer, 0, buffer.Length);
                    mipMaps[i].MipSize = (uint)BitConverter.ToInt32(buffer, 0); //Tu nie by³o rzutowania
                }

                // odejmujemy 8 bo rozmiar zawiera 8 bajtów dodatkowych, 4 magii i 4 rozmiaru mipampy
                buffer = new byte[mipMaps[i].Length - 8]; 
                stream.Read(buffer, 0, buffer.Length);

                if (compressed)
                {
                    ICompressor comp = new ZlibCompressor();
                    buffer = comp.Decompress(buffer);
                }

                mipMaps[i].Content = buffer;
            }
        }

        private void SetMipMapsDimensions(IList<TgvMipMap> mipMaps, uint imageWidth, uint imageHeight)
        {
            var sortedMipMaps = mipMaps.OrderByDescending(x => x.MipSize).ToList();
            for (int i = 0; i < sortedMipMaps.Count; i++)
            {
                uint scale = (uint)Math.Max(1, 2 << (i - 1));
                uint width = Math.Max(1, imageWidth / scale);
                uint height = Math.Max(1, imageHeight / scale);

                sortedMipMaps[i].MipWidth = width;
                sortedMipMaps[i].MipHeight = height;
            }
        }

    }
}
