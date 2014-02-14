using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Utilities;
using WargameModInstaller.Utilities.Compression;

namespace WargameModInstaller.Infrastructure.Image
{
    public class TgvRawStreamWriter : ITgvWriter
    {
        public const uint VersionMagic = 0x00000002;

        public TgvRawStreamWriter(Stream stream)
        {
            this.Stream = stream;
        }

        protected Stream Stream
        {
            get;
            private set;
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
        public virtual void Write(TgvImage file)
        {
            file.PixelFormatString = WargameModInstaller.Utilities.Image.TGV.TgvUtilities.GetTgvFromPixelFormat(file.Format); //"DXT5_SRGB"

            var buffer = BitConverter.GetBytes(VersionMagic);
            Stream.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes(file.IsCompressed ? 1 : 0);
            Stream.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes(file.Width);
            Stream.Write(buffer, 0, buffer.Length);
            buffer = BitConverter.GetBytes(file.Height);
            Stream.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes(file.Width);
            Stream.Write(buffer, 0, buffer.Length);
            buffer = BitConverter.GetBytes(file.Height);
            Stream.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes((short)file.MipMapCount);
            Stream.Write(buffer, 0, buffer.Length);

            var fmtLen = (short)file.PixelFormatString.Length;
            buffer = BitConverter.GetBytes(fmtLen);
            Stream.Write(buffer, 0, buffer.Length);

            buffer = Encoding.ASCII.GetBytes(file.PixelFormatString);
            Stream.Write(buffer, 0, buffer.Length);
            Stream.Seek(MiscUtilities.RoundToNextDivBy4(fmtLen) - fmtLen, SeekOrigin.Current);

            Stream.Write(file.SourceChecksum, 0, file.SourceChecksum.Length);

            var mipdefOffset = (uint)(Stream.Position);

            var mipImgsizes = new List<int>();
            var tileSize = file.Width - file.Width / file.MipMapCount;

            for (int i = 0; i < file.MipMapCount; i++)
            {
                Stream.Seek(8, SeekOrigin.Current);
                mipImgsizes.Add((int)tileSize);
                tileSize /= 4;
            }

            var sortedMipMaps = file.MipMaps.OrderBy(x => x.Content.Length).ToList();

            mipImgsizes = mipImgsizes.OrderBy(x => x).ToList();

            // Create the content and write all MipMaps, 
            // since we compress on this part its the first part where we know the size of a MipMap
            foreach (var sortedMipMap in sortedMipMaps)
            {
                sortedMipMap.Offset = (uint)Stream.Position;
                if (file.IsCompressed)
                {
                    var zipoMagic = Encoding.ASCII.GetBytes("ZIPO");
                    Stream.Write(zipoMagic, 0, zipoMagic.Length);

                    //buffer = BitConverter.GetBytes(mipImgsizes[sortedMipMaps.IndexOf(sortedMipMap)]);
                    buffer = BitConverter.GetBytes((int)Math.Pow(4, sortedMipMaps.IndexOf(sortedMipMap)));
                    Stream.Write(buffer, 0, buffer.Length);

                    ICompressor comp = new ZlibCompressor();
                    buffer = comp.Compress(sortedMipMap.Content);
                    Stream.Write(buffer, 0, buffer.Length);
                    sortedMipMap.Size = (uint)buffer.Length;
                }
                else
                {
                    Stream.Write(sortedMipMap.Content, 0, sortedMipMap.Content.Length);
                    sortedMipMap.Size = (uint)sortedMipMap.Content.Length;
                }
            }

            Stream.Seek(mipdefOffset, SeekOrigin.Begin);

            // Write the offset collection in the header.
            for (int i = 0; i < file.MipMapCount; i++)
            {
                buffer = BitConverter.GetBytes(sortedMipMaps[i].Offset);
                Stream.Write(buffer, 0, buffer.Length);
            }

            // Write the size collection into the header.
            for (int i = 0; i < file.MipMapCount; i++)
            {
                buffer = BitConverter.GetBytes(sortedMipMaps[i].Size + 8);
                Stream.Write(buffer, 0, buffer.Length);
            }
        }

    }
}
