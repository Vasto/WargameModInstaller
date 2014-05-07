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
    }

}
