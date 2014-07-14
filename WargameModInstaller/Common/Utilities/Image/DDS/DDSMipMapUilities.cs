using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Utilities.Image.DDS
{
    public static class DDSMipMapUilities
    {
        private static Dictionary<FourCC, uint> knownMinSizesMap = new Dictionary<FourCC, uint>()
        {
            { DDSFormat.PixelFormat.DXT1.FourCC, 8 },
            { DDSFormat.PixelFormat.DXT2.FourCC, 16 },
            { DDSFormat.PixelFormat.DXT3.FourCC, 16 },
            { DDSFormat.PixelFormat.DXT4.FourCC, 16 },
            { DDSFormat.PixelFormat.DXT5.FourCC, 16 },
        };

        public static uint GetMinimumMipMapSizeForFormat(DDSFormat.PixelFormat pixelFormat)
        {
            if(knownMinSizesMap.ContainsKey(pixelFormat.FourCC))
            {
                return knownMinSizesMap[pixelFormat.FourCC];
            }
            else
            {
                return 0;
            }
        }

        public static uint GetMipMapDimension(uint imageDimension, uint mipMapIndex)
        {
            uint mipMapDiemension = imageDimension;

            for (int i = 0; i < mipMapIndex; ++i)
            {
                mipMapDiemension /= 2;
            }

            return mipMapDiemension;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mipWidth"></param>
        /// <param name="mipHeight"></param>
        /// <param name="format"></param>
        /// <returns>// The following code is a port of DirectXTex</returns>
        /// <see cref="http://directxtex.codeplex.com"/>
        public static int GetMipMapBytesCount(int mipWidth, int mipHeight, PixelFormats format)
        {    
            bool bc = false;
            bool packed = false;
            bool planar = false;
            int bpe = 0;

            switch (format)
            {
                case PixelFormats.BC1_TYPELESS:
                case PixelFormats.BC1_UNORM:
                case PixelFormats.BC1_UNORM_SRGB:
                case PixelFormats.BC4_TYPELESS:
                case PixelFormats.BC4_UNORM:
                case PixelFormats.BC4_SNORM:
                    bc = true;
                    bpe = 8;
                    break;

                case PixelFormats.BC2_TYPELESS:
                case PixelFormats.BC2_UNORM:
                case PixelFormats.BC2_UNORM_SRGB:
                case PixelFormats.BC3_TYPELESS:
                case PixelFormats.BC3_UNORM:
                case PixelFormats.BC3_UNORM_SRGB:
                case PixelFormats.BC5_TYPELESS:
                case PixelFormats.BC5_UNORM:
                case PixelFormats.BC5_SNORM:
                case PixelFormats.BC6H_TYPELESS:
                case PixelFormats.BC6H_UF16:
                case PixelFormats.BC6H_SF16:
                case PixelFormats.BC7_TYPELESS:
                case PixelFormats.BC7_UNORM:
                case PixelFormats.BC7_UNORM_SRGB:
                    bc = true;
                    bpe = 16;
                    break;

                case PixelFormats.R8G8_B8G8_UNORM:
                case PixelFormats.G8R8_G8B8_UNORM:
                case PixelFormats.YUY2:
                    packed = true;
                    bpe = 4;
                    break;

                case PixelFormats.Y210:
                case PixelFormats.Y216:
                    packed = true;
                    bpe = 8;
                    break;

                case PixelFormats.NV12:
                    planar = true;
                    bpe = 2;
                    break;

                case PixelFormats.P010:
                case PixelFormats.P016:
                    planar = true;
                    bpe = 4;
                    break;
            }

            if (bc)
            {
                int numBlocksWide = 0;
                if (mipWidth > 0)
                {
                    numBlocksWide = Math.Max( 1, (mipWidth + 3) / 4 );
                }
                int numBlocksHigh = 0;
                if (mipHeight > 0)
                {
                    numBlocksHigh = Math.Max( 1, (mipHeight + 3) / 4 );
                }
                int rowBytes = numBlocksWide * bpe;
                //int numRows = numBlocksHigh;
                int numBytes = rowBytes * numBlocksHigh;

                return numBytes;
            }
            else if (packed)
            {
                int rowBytes = ((mipWidth + 1) >> 1) * bpe;
                //int numRows = mipHeight;
                int numBytes = rowBytes * mipHeight;

                return numBytes;
            }
            else if (format == PixelFormats.NV11)
            {
                int rowBytes = ((mipWidth + 3) >> 2) * 4;
                // Direct3D makes this simplifying assumption, although it is larger than the 4:1:1 data
                int numRows = mipHeight * 2; 
                int numBytes = rowBytes * numRows;

                return numBytes;
            }
            else if (planar)
            {
                int rowBytes = ((mipWidth + 1) >> 1) * bpe;
                int numBytes = (rowBytes * mipHeight) + ((rowBytes * mipHeight + 1) >> 1);
                //int numRows = mipHeight + ((mipHeight + 1) >> 1);

                return numBytes;
            }
            else
            {
                int bpp = DDSFormat.GetBitsPerPixel(format);
                // round up to nearest byte
                int rowBytes = (mipWidth * bpp + 7) / 8;
                int numBytes = rowBytes * mipHeight;

                return numBytes;
            }
        }

    }

}
