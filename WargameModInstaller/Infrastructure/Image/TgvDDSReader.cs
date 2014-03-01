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
    public class TgvDDSReader : ITgvFileReader
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
            byte[] rawDDSData = File.ReadAllBytes(filePath);

            int contentSize = rawDDSData.Length - Marshal.SizeOf(typeof(DDSFormat.Header)) - Marshal.SizeOf((typeof(uint)));

            var file = new TgvImage();

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
                int mipSize = contentSize;
                if (header.MipMapCount == 0)
                {
                    header.MipMapCount = 1;
                }
                else if (header.MipMapCount > 1) //When equal 1 it would make a 0 length content mipmap
                {
                    mipSize -= contentSize / header.MipMapCount;
                }

                for (ushort i = 0; i < header.MipMapCount; i++)
                {
                    buffer = new byte[mipSize];
                    ms.Read(buffer, 0, buffer.Length);

                    var mip = new TgvMipMap { Content = buffer };
                    file.MipMaps.Add(mip);

                    mipSize /= 4;
                }

                file.Height = header.Height;
                file.ImageHeight = header.Height;
                file.Width = header.Width;
                file.ImageHeight = header.Width;
                file.MipMapCount = (ushort)header.MipMapCount;

                DDSHelper.ConversionFlags conversionFlags;
                file.Format = DDSHelper.GetDXGIFormat(ref header.PixelFormat, out conversionFlags);
            }

            return file;

        }

    }
}
