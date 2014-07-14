using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Common.Utilities.Image.DDS;
using WargameModInstaller.Common.Utilities.Image.TGV;

namespace WargameModInstaller.Infrastructure.Image
{
    /// <summary>
    /// Represents a reader which can read a TGV image from a DDS file. 
    /// This reader ignores any additional MipMap images during the file reading.
    /// </summary>
    public class TgvDDSMoMipMapsReader : ITgvFileReader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public virtual TgvImage Read(String filePath)
        {
            var file = new TgvImage();

            byte[] rawDDSData = File.ReadAllBytes(filePath);
            using (var ms = new MemoryStream(rawDDSData))
            {
                var buffer = new byte[4];
                ms.Read(buffer, 0, buffer.Length);

                if (BitConverter.ToUInt32(buffer, 0) != DDSFormat.MagicHeader)
                {
                    throw new ArgumentException("Wrong DDS magic");
                }

                buffer = new byte[Marshal.SizeOf(typeof(DDSFormat.Header))];
                ms.Read(buffer, 0, buffer.Length);

                var header = MiscUtilities.ByteArrayToStructure<DDSFormat.Header>(buffer);
                header.MipMapCount = 1;

                DDSHelper.ConversionFlags conversionFlags;
                var format = DDSHelper.GetDXGIFormat(ref header.PixelFormat, out conversionFlags);

                //read only the main content mipmap
                uint minMipByteLength = DDSMipMapUilities.GetMinimumMipMapSizeForFormat(header.PixelFormat);
                uint mipByteLength = (uint)DDSMipMapUilities.GetMipMapBytesCount((int)header.Width, (int)header.Height, format);
                mipByteLength = Math.Max(minMipByteLength, mipByteLength);

                buffer = new byte[mipByteLength];
                ms.Read(buffer, 0, buffer.Length);

                var mip = new TgvMipMap();
                mip.Content = buffer;
                mip.Length = mipByteLength;
                mip.MipSize = header.Width * header.Height;
                mip.MipWidth = header.Width;
                mip.MipHeight = header.Height; 

                file.MipMapCount = (ushort)header.MipMapCount;
                file.MipMaps.Add(mip);
                file.Height = header.Height;
                file.ImageHeight = header.Height;
                file.Width = header.Width;
                file.ImageWidth = header.Width;
                file.Format = format;
                file.PixelFormatString = TgvUtilities.GetTgvFromPixelFormat(format);
            }

            return file;

        }

    }
}
