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

                for (int i = 0; i < header.MipMapCount; i++)
                {
                    uint mipScale = (uint)Math.Max(1, 2 << (i - 1));
                    uint mipWidth = Math.Max(1, header.Width / mipScale);
                    uint mipHeight = Math.Max(1, header.Height / mipScale);

                    if (mipWidth < MinMipMapWidth || mipHeight < MinMipMapHeight)
                    {
                        break;
                    }

                    uint minMipByteLength = DDSMipMapUilities.GetMinimumMipMapSizeForFormat(header.PixelFormat);
                    uint mipByteLength = Math.Max(minMipByteLength, mipWidth * mipHeight);

                    buffer = new byte[mipByteLength];
                    ms.Read(buffer, 0, buffer.Length);

                    var mip = new TgvMipMap();
                    mip.Content = buffer;
                    mip.Length = mipByteLength;
                    mip.MipSize = mipWidth * mipHeight;//(int)mipSize; //spr. czy to jest równe size czy width * height;
                    mip.MipWidth = mipWidth;
                    mip.MipHeight = mipHeight; 

                    file.MipMaps.Add(mip);
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
