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
    /// <summary>
    /// Reperesents a writer which can write a TGV image to a byte form.
    /// </summary>
    public class TgvBinWriter : ITgvBinWriter
    {
        protected const uint VersionMagic = 0x00000002;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
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

                var mipOffset = (uint)(stream.Position);

                stream.Seek(8 * file.MipMapCount, SeekOrigin.Current);

                var sortedMipMaps = file.MipMaps.OrderBy(x => x.Content.Length).ToList();

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
                        //To Ÿle dzia³a. W orygianlnym pliku bajty s¹ w innej kolejnoœci. Pozatym to zak³ada ze potêg¹ jest indeks, 
                        //a w przypadku ma³ej liczby bitmap to nie ma sensu
                        uint mipSize = GetMipMapSize(file.Width, file.Height, sortedMipMaps.IndexOf(sortedMipMap), sortedMipMaps.Count);
                        buffer = BitConverter.GetBytes(mipSize); 
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

                    //Wygl¹da na to ¿e mipMapy musza mieæ offset wzglêden pocz¹tku pliku podzielny przez 4.
                    //Trzeba dope³niæ do liczby podzielnej przez 4 przed zapisem nastêpnego
                    //To dope³nienie zak³ada ¿e nag³ówek ma d³ugoœæ podzielna przez 4, tak ¿e pierwsza MipMapa nie jest przesuniêta.

                    bool multiMipMaps = file.MipMaps.Count > 1;
                    bool needSupplementTo4 = (stream.Position % 4) != 0;
                    if (multiMipMaps && needSupplementTo4)
                    {
                        int supplementSize = (int)(MiscUtilities.RoundToNextDivBy4(stream.Position) - stream.Position);
                        byte[] supplementBuffer = new byte[supplementSize];
                        stream.Write(supplementBuffer, 0, supplementBuffer.Length);
                    }
                }

                stream.Seek(mipOffset, SeekOrigin.Begin);

                // Write the offset collection in the header.
                for (int i = 0; i < file.MipMapCount; i++)
                {
                    buffer = BitConverter.GetBytes(sortedMipMaps[i].Offset);
                    stream.Write(buffer, 0, buffer.Length);
                }

                // Write the size collection into the header.
                for (int i = 0; i < file.MipMapCount; i++)
                {
                    buffer = BitConverter.GetBytes(sortedMipMaps[i].Size + 8); //+ 4 magii i + 4 rozmiaru mapy
                    stream.Write(buffer, 0, buffer.Length);
                }

                return stream.ToArray();
            }
        }

        private uint GetMipMapSize(uint imageWidth, uint imageHeight, int mipMapIndex, int mipMapsCount)
        {
            if (mipMapIndex == mipMapsCount - 1)
            {
                return imageWidth * imageHeight;
            }

            uint mipWidth = imageWidth;
            uint mipHeight = imageHeight;
            for (int i = mipMapsCount - 1; i > mipMapIndex; --i)
            {
                mipWidth /= 2;
                mipHeight /= 2;
            }

            return mipWidth * mipHeight;
        }


    }

}
