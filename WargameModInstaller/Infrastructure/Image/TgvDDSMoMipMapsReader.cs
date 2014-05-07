using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Utilities.Image.DDS;

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

                //read only the main content mipmap
                uint mainContentMipMapSize = header.Width * header.Height; 
                buffer = new byte[mainContentMipMapSize];
                ms.Read(buffer, 0, buffer.Length);

                var mainContentMipMap = new TgvMipMap();
                mainContentMipMap.Content = buffer;

                file.MipMapCount = (ushort)header.MipMapCount;
                file.MipMaps.Add(mainContentMipMap);
                file.Height = header.Height;
                file.ImageHeight = header.Height;
                file.Width = header.Width;
                file.ImageHeight = header.Width;

                DDSHelper.ConversionFlags conversionFlags;
                file.Format = DDSHelper.GetDXGIFormat(ref header.PixelFormat, out conversionFlags);
            }

            return file;

        }

    }
}
