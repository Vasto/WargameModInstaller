using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Common.Utilities.Image.DDS;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Infrastructure.Image
{
    /// <summary>
    /// Represents a reader which can read a TGV imgae from a DDS file.
    /// </summary>
    public class TgvDDSReader : ITgvFileReader
    {
        public TgvDDSReader()
        {
            //Those are minimum values for the default in-Wargame mipmaps
            this.MinMipMapWidth = 4;
            this.MinMipMapHeight = 4;
        }

        public uint MinMipMapWidth
        {
            get;
            set;
        }

        public uint MinMipMapHeight
        {
            get;
            set;
        }

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
                if (header.MipMapCount == 0)
                {
                    header.MipMapCount = 1;
                }

                uint mipWidth = header.Width;
                uint mipHeight = header.Height;
                uint mipSize = mipWidth * mipHeight;
                for (ushort i = 0; i < header.MipMapCount; i++)
                {
                    if (mipWidth < MinMipMapWidth || mipHeight < MinMipMapHeight)
                    {
                        break;
                    }

                    uint minMipMapSize = DDSMipMapUilities.GetMinimumMipMapSizeForFormat(header.PixelFormat);
                    mipSize = Math.Max(minMipMapSize, mipSize);

                    buffer = new byte[mipSize];
                    ms.Read(buffer, 0, buffer.Length);

                    var mip = new TgvMipMap();
                    mip.Content = buffer;
                    mip.Size = mipSize;
                    file.MipMaps.Add(mip);

                    mipWidth /= 2;
                    mipHeight /= 2;
                    mipSize = mipWidth * mipHeight;
                }

                file.Height = header.Height;
                file.ImageHeight = header.Height;
                file.Width = header.Width;
                file.ImageHeight = header.Width;
                file.MipMapCount = (ushort)file.MipMaps.Count;

                DDSHelper.ConversionFlags conversionFlags;
                file.Format = DDSHelper.GetDXGIFormat(ref header.PixelFormat, out conversionFlags);
            }

            return file;
        }


    }
}
