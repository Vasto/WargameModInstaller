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
    public class TgvBinWriter : ITgvBinWriter
    {
        protected const uint VersionMagic = 0x00000002;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public virtual byte[] Write(TgvImage file)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                file.PixelFormatString = TgvUtilities.GetTgvFromPixelFormat(file.Format); //"DXT5_SRGB"

                var buffer = BitConverter.GetBytes(VersionMagic);
                stream.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(file.IsCompressed ? 1 : 0);
                stream.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(file.Width);
                stream.Write(buffer, 0, buffer.Length);
                buffer = BitConverter.GetBytes(file.Height);
                stream.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(file.Width);
                stream.Write(buffer, 0, buffer.Length);
                buffer = BitConverter.GetBytes(file.Height);
                stream.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes((short)file.MipMapCount);
                stream.Write(buffer, 0, buffer.Length);

                var fmtLen = (short)file.PixelFormatString.Length;
                buffer = BitConverter.GetBytes(fmtLen);
                stream.Write(buffer, 0, buffer.Length);

                buffer = Encoding.ASCII.GetBytes(file.PixelFormatString);
                stream.Write(buffer, 0, buffer.Length);
                stream.Seek(MiscUtilities.RoundToNextDivBy4(fmtLen) - fmtLen, SeekOrigin.Current);

                stream.Write(file.SourceChecksum, 0, file.SourceChecksum.Length);

                var mipdefOffset = (uint)(stream.Position);

                var mipImgsizes = new List<int>();
                var tileSize = file.Width - file.Width / file.MipMapCount;

                for (int i = 0; i < file.MipMapCount; i++)
                {
                    stream.Seek(8, SeekOrigin.Current);
                    mipImgsizes.Add((int)tileSize);
                    tileSize /= 4;
                }

                var sortedMipMaps = file.MipMaps.OrderBy(x => x.Content.Length).ToList();

                mipImgsizes = mipImgsizes.OrderBy(x => x).ToList();

                // Create the content and write all MipMaps, 
                // since we compress on this part its the first part where we know the size of a MipMap
                foreach (var sortedMipMap in sortedMipMaps)
                {
                    sortedMipMap.Offset = (uint)stream.Position;
                    if (file.IsCompressed)
                    {
                        var zipoMagic = Encoding.ASCII.GetBytes("ZIPO");
                        stream.Write(zipoMagic, 0, zipoMagic.Length);

                        //buffer = BitConverter.GetBytes(mipImgsizes[sortedMipMaps.IndexOf(sortedMipMap)]);
                        buffer = BitConverter.GetBytes((int)Math.Pow(4, sortedMipMaps.IndexOf(sortedMipMap)));
                        stream.Write(buffer, 0, buffer.Length);

                        ICompressor comp = new ZlibCompressor();
                        buffer = comp.Compress(sortedMipMap.Content);
                        stream.Write(buffer, 0, buffer.Length);
                        sortedMipMap.Size = (uint)buffer.Length;
                    }
                    else
                    {
                        stream.Write(sortedMipMap.Content, 0, sortedMipMap.Content.Length);
                        sortedMipMap.Size = (uint)sortedMipMap.Content.Length;
                    }
                }

                stream.Seek(mipdefOffset, SeekOrigin.Begin);

                // Write the offset collection in the header.
                for (int i = 0; i < file.MipMapCount; i++)
                {
                    buffer = BitConverter.GetBytes(sortedMipMaps[i].Offset);
                    stream.Write(buffer, 0, buffer.Length);
                }

                // Write the size collection into the header.
                for (int i = 0; i < file.MipMapCount; i++)
                {
                    buffer = BitConverter.GetBytes(sortedMipMaps[i].Size + 8);
                    stream.Write(buffer, 0, buffer.Length);
                }

                return stream.ToArray();
            }

        }

    }

}
