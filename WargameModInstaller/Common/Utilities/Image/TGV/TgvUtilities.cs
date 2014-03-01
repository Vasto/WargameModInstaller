using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Utilities.Image.DDS;

namespace WargameModInstaller.Utilities.Image.TGV
{
    public static class TgvUtilities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        /// <remarks>
        /// Credits to enohka for this code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public static PixelFormats GetPixelFormatFromTgv(String pixelFormat)
        {
            switch (pixelFormat)
            {
                case "A8R8G8B8_HDR":
                case "A8R8G8B8_LIN":
                case "A8R8G8B8_LIN_HDR":
                case "A8R8G8B8":
                    return PixelFormats.R8G8B8A8_UNORM;
                case "X8R8G8B8":
                case "X8R8G8B8_LE":
                    return PixelFormats.B8G8R8X8_UNORM;
                case "X8R8G8B8_SRGB":
                    return PixelFormats.B8G8R8X8_UNORM_SRGB;

                case "A8R8G8B8_SRGB":
                case "A8R8G8B8_SRGB_HDR":
                    return PixelFormats.R8G8B8A8_UNORM_SRGB;

                case "A16B16G16R16":
                case "A16B16G16R16_EDRAM":
                    return PixelFormats.R16G16B16A16_UNORM;

                case "A16B16G16R16F":
                case "A16B16G16R16F_LIN":
                    return PixelFormats.R16G16B16A16_FLOAT;

                case "A32B32G32R32F":
                case "A32B32G32R32F_LIN":
                    return PixelFormats.R32G32B32A32_FLOAT;

                case "A8":
                case "A8_LIN":
                    return PixelFormats.A8_UNORM;
                case "A8P8":
                    return PixelFormats.A8P8;
                case "P8":
                    return PixelFormats.P8;
                case "L8":
                case "L8_LIN":
                    return PixelFormats.R8_UNORM;
                case "L16":
                case "L16_LIN":
                    return PixelFormats.R16_UNORM;
                case "D16_LOCKABLE":
                case "D16":
                case "D16F":
                    return PixelFormats.D16_UNORM;
                case "V8U8":
                    return PixelFormats.R8G8_SNORM;
                case "V16U16":
                    return PixelFormats.R16G16_SNORM;

                case "DXT1":
                case "DXT1_LIN":
                    return PixelFormats.BC1_UNORM;
                case "DXT1_SRGB":
                    return PixelFormats.BC1_UNORM_SRGB;
                   // return PixelFormats.BC1_UNORM;
                case "DXT2":
                case "DXT3":
                case "DXT3_LIN":
                    return PixelFormats.BC2_UNORM;
                case "DXT3_SRGB":
                    return PixelFormats.BC2_UNORM_SRGB;
                case "DXT4":
                case "DXT5":
                case "DXT5_LIN":
                case "DXT5_FROM_ENCODE":
                    return PixelFormats.BC3_UNORM;
                case "DXT5_SRGB":
                    return PixelFormats.BC3_UNORM_SRGB;

                case "R5G6B5_LIN":
                case "R5G6B5":
                case "R8G8B8":
                case "X1R5G5B5":
                case "X1R5G5B5_LIN":
                case "A1R5G5B5":
                case "A4R4G4B4":
                case "R3G3B2":
                case "A8R3G3B2":
                case "X4R4G4B4":
                case "A8L8":
                case "A4L4":
                case "L6V5U5":
                case "X8L8V8U8":
                case "Q8W8U8V8":
                case "W11V11U10":
                case "UYVY":
                case "YUY2":
                case "D32":
                case "D32F_LOCKABLE":
                case "D15S1":
                case "D24S8":
                case "R16F":
                case "R32F":
                case "R32F_LIN":
                case "A2R10G10B10":
                case "D24X8":
                case "D24X8F":
                case "D24X4S4":
                case "G16R16":
                case "G16R16_EDRAM":
                case "G16R16F":
                case "G16R16F_LIN":
                case "G32R32F":
                case "G32R32F_LIN":
                case "A2R10G10B10_LE":
                case "CTX1":
                case "CTX1_LIN":
                case "DXN":
                case "DXN_LIN":
                case "INTZ":
                case "RAWZ":
                case "DF24":
                case "PIXNULL":
                    throw new NotSupportedException(string.Format("Pixelformat {0} not supported", pixelFormat));

                default:
                    throw new NotSupportedException(string.Format("Unknown Pixelformat {0} ", pixelFormat));
            }
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
        public static string GetTgvFromPixelFormat(PixelFormats pixelFormat)
        {
            switch (pixelFormat)
            {
                case PixelFormats.R8G8B8A8_UNORM:
                    return "A8R8G8B8";
                case PixelFormats.B8G8R8X8_UNORM:
                    return "X8R8G8B8";
                case PixelFormats.B8G8R8X8_UNORM_SRGB:
                    return "X8R8G8B8_SRGB";

                case PixelFormats.R8G8B8A8_UNORM_SRGB:
                    return "A8R8G8B8_SRGB";

                case PixelFormats.R16G16B16A16_UNORM:
                    return "A16B16G16R16";

                case PixelFormats.R16G16B16A16_FLOAT:
                    return "A16B16G16R16F";

                case PixelFormats.R32G32B32A32_FLOAT:
                    return "A32B32G32R32F";

                case PixelFormats.A8_UNORM:
                    return "A8";
                case PixelFormats.A8P8:
                    return "A8P8";
                case PixelFormats.P8:
                    return "P8";
                case PixelFormats.R8_UNORM:
                    return "L8";
                case PixelFormats.R16_UNORM:
                    return "L16";
                case PixelFormats.D16_UNORM:
                    return "D16";
                case PixelFormats.R8G8_SNORM:
                    return "V8U8";
                case PixelFormats.R16G16_SNORM:
                    return "V16U16";

                case PixelFormats.BC1_UNORM:
                    return "DXT1";
                case PixelFormats.BC1_UNORM_SRGB:
                    return "DXT1_SRGB";
                    //return "DXT1";
                case PixelFormats.BC2_UNORM:
                    return "DXT3";
                case PixelFormats.BC2_UNORM_SRGB:
                    return "DXT3_SRGB";
                case PixelFormats.BC3_UNORM:
                    return "DXT5";
                case PixelFormats.BC3_UNORM_SRGB:
                    return "DXT5_SRGB";

                default:
                    throw new NotSupportedException(string.Format("Unsupported PixelFormat {0}", pixelFormat));
            }
        }
    }
}
