// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// The following code is a port of DirectXTex http://directxtex.codeplex.com
// -----------------------------------------------------------------------------
// Microsoft Public License (Ms-PL)
//
// This license governs use of the accompanying software. If you use the
// software, you accept this license. If you do not accept the license, do not
// use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and
// "distribution" have the same meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to
// the software.
// A "contributor" is any person that distributes its contribution under this
// license.
// "Licensed patents" are a contributor's patent claims that read directly on
// its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the
// license conditions and limitations in section 3, each contributor grants
// you a non-exclusive, worldwide, royalty-free copyright license to reproduce
// its contribution, prepare derivative works of its contribution, and
// distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license
// conditions and limitations in section 3, each contributor grants you a
// non-exclusive, worldwide, royalty-free license under its licensed patents to
// make, have made, use, sell, offer for sale, import, and/or otherwise dispose
// of its contribution in the software or derivative works of the contribution
// in the software.
//
// 3. Conditions and Limitations
// (A) No Trademark License- This license does not grant you rights to use any
// contributors' name, logo, or trademarks.
// (B) If you bring a patent claim against any contributor over patents that
// you claim are infringed by the software, your patent license from such
// contributor to the software ends automatically.
// (C) If you distribute any portion of the software, you must retain all
// copyright, patent, trademark, and attribution notices that are present in the
// software.
// (D) If you distribute any portion of the software in source code form, you
// may do so only under this license by including a complete copy of this
// license with your distribution. If you distribute any portion of the software
// in compiled or object code form, you may only do so under a license that
// complies with this license.
// (E) The software is licensed "as-is." You bear the risk of using it. The
// contributors give no express warranties, guarantees or conditions. You may
// have additional consumer rights under your local laws which this license
// cannot change. To the extent permitted under your local laws, the
// contributors exclude the implied warranties of merchantability, fitness for a
// particular purpose and non-infringement.



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WargameModInstaller.Common.Utilities.Image.DDS
{
    public class DDSHelper
    {
        [Flags]
        public enum ConversionFlags
        {
            None = 0x0,
            Expand = 0x1, // Conversion requires expanded pixel size
            NoAlpha = 0x2, // Conversion requires setting alpha to known value
            Swizzle = 0x4, // BGR/RGB order swizzling required
            Pal8 = 0x8, // Has an 8-bit palette
            Format888 = 0x10, // Source is an 8:8:8 (24bpp) format
            Format565 = 0x20, // Source is a 5:6:5 (16bpp) format
            Format5551 = 0x40, // Source is a 5:5:5:1 (16bpp) format
            Format4444 = 0x80, // Source is a 4:4:4:4 (16bpp) format
            Format44 = 0x100, // Source is a 4:4 (8bpp) format
            Format332 = 0x200, // Source is a 3:3:2 (8bpp) format
            Format8332 = 0x400, // Source is a 8:3:3:2 (16bpp) format
            FormatA8P8 = 0x800, // Has an 8-bit palette with an alpha channel
            DX10 = 0x10000, // Has the 'DX10' extension header
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct LegacyMap
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="LegacyMap" /> struct.
            /// </summary>
            /// <param name="format">The format.</param>
            /// <param name="conversionFlags">The conversion flags.</param>
            /// <param name="pixelFormat">The pixel format.</param>
            public LegacyMap(PixelFormats format, ConversionFlags conversionFlags, DDSFormat.PixelFormat pixelFormat)
            {
                Format = format;
                ConversionFlags = conversionFlags;
                PixelFormat = pixelFormat;
            }

            public PixelFormats Format;
            public ConversionFlags ConversionFlags;
            public DDSFormat.PixelFormat PixelFormat;
        };

        private static readonly LegacyMap[] LegacyMaps = new[]
        {
            //new LegacyMap(PixelFormats.BC1_UNORM_SRGB, ConversionFlags.None, DDSFormat.PixelFormat.DXT1), // D3DFMT_DXT1
            new LegacyMap(PixelFormats.BC1_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.DXT1), // D3DFMT_DXT1
            new LegacyMap(PixelFormats.BC2_UNORM_SRGB, ConversionFlags.None, DDSFormat.PixelFormat.DXT3), // D3DFMT_DXT3
            new LegacyMap(PixelFormats.BC3_UNORM_SRGB, ConversionFlags.None, DDSFormat.PixelFormat.DXT5), // D3DFMT_DXT5

            new LegacyMap(PixelFormats.BC2_UNORM_SRGB, ConversionFlags.None, DDSFormat.PixelFormat.DXT2), // D3DFMT_DXT2 (ignore premultiply)
            new LegacyMap(PixelFormats.BC3_UNORM_SRGB, ConversionFlags.None, DDSFormat.PixelFormat.DXT4), // D3DFMT_DXT4 (ignore premultiply)

            new LegacyMap(PixelFormats.BC4_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.BC4_UNorm),
            new LegacyMap(PixelFormats.BC4_SNORM, ConversionFlags.None, DDSFormat.PixelFormat.BC4_SNorm),
            new LegacyMap(PixelFormats.BC5_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.BC5_UNorm),
            new LegacyMap(PixelFormats.BC5_SNORM, ConversionFlags.None, DDSFormat.PixelFormat.BC5_SNorm),

            new LegacyMap(PixelFormats.BC4_UNORM, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.FourCC, new FourCC('A', 'T', 'I', '1'), 0, 0, 0, 0, 0)),
            new LegacyMap(PixelFormats.BC5_UNORM, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.FourCC, new FourCC('A', 'T', 'I', '2'), 0, 0, 0, 0, 0)),
            new LegacyMap(PixelFormats.R8G8_B8G8_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.R8G8_B8G8), // D3DFMT_R8G8_B8G8
            new LegacyMap(PixelFormats.G8R8_G8B8_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.G8R8_G8B8), // D3DFMT_G8R8_G8B8
            new LegacyMap(PixelFormats.B8G8R8A8_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.A8R8G8B8), // D3DFMT_A8R8G8B8 (uses DXGI 1.1 format)
            new LegacyMap(PixelFormats.B8G8R8X8_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.X8R8G8B8), // D3DFMT_X8R8G8B8 (uses DXGI 1.1 format)
            new LegacyMap(PixelFormats.R8G8B8A8_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.A8B8G8R8), // D3DFMT_A8B8G8R8
            new LegacyMap(PixelFormats.R8G8B8A8_UNORM, ConversionFlags.NoAlpha, DDSFormat.PixelFormat.X8B8G8R8), // D3DFMT_X8B8G8R8
            new LegacyMap(PixelFormats.R16G16_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.G16R16), // D3DFMT_G16R16

            new LegacyMap(PixelFormats.R10G10B10A2_UNORM, ConversionFlags.Swizzle, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.Rgb, 0, 32, 0x000003ff, 0x000ffc00, 0x3ff00000, 0xc0000000)),
            // D3DFMT_A2R10G10B10 (D3DX reversal issue workaround)
            new LegacyMap(PixelFormats.R10G10B10A2_UNORM, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.Rgb, 0, 32, 0x3ff00000, 0x000ffc00, 0x000003ff, 0xc0000000)),
            // D3DFMT_A2B10G10R10 (D3DX reversal issue workaround)

            new LegacyMap(PixelFormats.R8G8B8A8_UNORM, ConversionFlags.Expand
                | ConversionFlags.NoAlpha
                | ConversionFlags.Format888, DDSFormat.PixelFormat.R8G8B8), // D3DFMT_R8G8B8

            new LegacyMap(PixelFormats.B5G6R5_UNORM, ConversionFlags.Format565, DDSFormat.PixelFormat.R5G6B5), // D3DFMT_R5G6B5
            new LegacyMap(PixelFormats.B5G5R5A1_UNORM, ConversionFlags.Format5551, DDSFormat.PixelFormat.A1R5G5B5), // D3DFMT_A1R5G5B5
            new LegacyMap(PixelFormats.B5G5R5A1_UNORM, ConversionFlags.Format5551
                | ConversionFlags.NoAlpha, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.Rgb, 0, 16, 0x7c00, 0x03e0, 0x001f, 0x0000)), // D3DFMT_X1R5G5B5
     
            new LegacyMap(PixelFormats.R8G8B8A8_UNORM, ConversionFlags.Expand
                | ConversionFlags.Format8332, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.Rgb, 0, 16, 0x00e0, 0x001c, 0x0003, 0xff00)), // D3DFMT_A8R3G3B2
            new LegacyMap(PixelFormats.B5G6R5_UNORM, ConversionFlags.Expand
                | ConversionFlags.Format332, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.Rgb, 0, 8, 0xe0, 0x1c, 0x03, 0x00)), // D3DFMT_R3G3B2
 
            new LegacyMap(PixelFormats.R8_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.L8), // D3DFMT_L8
            new LegacyMap(PixelFormats.R16_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.L16), // D3DFMT_L16
            new LegacyMap(PixelFormats.R8G8_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.A8L8), // D3DFMT_A8L8

            new LegacyMap(PixelFormats.A8_UNORM, ConversionFlags.None, DDSFormat.PixelFormat.A8), // D3DFMT_A8

            new LegacyMap(PixelFormats.R16G16B16A16_UNORM, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.FourCC, 36, 0, 0, 0, 0, 0)), // D3DFMT_A16B16G16R16
            new LegacyMap(PixelFormats.R16G16B16A16_SNORM, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.FourCC, 110, 0, 0, 0, 0, 0)), // D3DFMT_Q16W16V16U16
            new LegacyMap(PixelFormats.R16_FLOAT, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.FourCC, 111, 0, 0, 0, 0, 0)), // D3DFMT_R16F
            new LegacyMap(PixelFormats.R16G16_FLOAT, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.FourCC, 112, 0, 0, 0, 0, 0)), // D3DFMT_G16R16F
            new LegacyMap(PixelFormats.R16G16B16A16_FLOAT, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.FourCC, 113, 0, 0, 0, 0, 0)), // D3DFMT_A16B16G16R16F
            new LegacyMap(PixelFormats.R32_FLOAT, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.FourCC, 114, 0, 0, 0, 0, 0)), // D3DFMT_R32F
            new LegacyMap(PixelFormats.R32G32_FLOAT, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.FourCC, 115, 0, 0, 0, 0, 0)), // D3DFMT_G32R32F
            new LegacyMap(PixelFormats.R32G32B32A32_FLOAT, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.FourCC, 116, 0, 0, 0, 0, 0)), // D3DFMT_A32B32G32R32F

            new LegacyMap(PixelFormats.R32_FLOAT, ConversionFlags.None, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.Rgb, 0, 32, 0xffffffff, 0x00000000, 0x00000000, 0x00000000)),
            // D3DFMT_R32F (D3DX uses FourCC 114 instead)

            new LegacyMap(PixelFormats.R8G8B8A8_UNORM, ConversionFlags.Expand
                | ConversionFlags.Pal8
                | ConversionFlags.FormatA8P8, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.Pal8, 0, 16, 0, 0, 0, 0)), // D3DFMT_A8P8
            new LegacyMap(PixelFormats.R8G8B8A8_UNORM, ConversionFlags.Expand
                | ConversionFlags.Pal8, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.Pal8, 0, 8, 0, 0, 0, 0)), // D3DFMT_P8
#if DIRECTX11_1
    new LegacyMap( PixelFormats.B4G4R4A4_UNorm,     ConversionFlags.Format4444,        DDS.PixelFormat.A4R4G4B4 ), // D3DFMT_A4R4G4B4 (uses DXGI 1.2 format)
    new LegacyMap( PixelFormats.B4G4R4A4_UNorm,     ConversionFlags.NoAlpha
                                      | ConversionFlags.Format4444,      new DDS.PixelFormat(DDS.PixelFormatFlags.Rgb,       0, 16, 0x0f00,     0x00f0,     0x000f,     0x0000     ) ), // D3DFMT_X4R4G4B4 (uses DXGI 1.2 format)
    new LegacyMap( PixelFormats.B4G4R4A4_UNorm,     ConversionFlags.Expand
                                      | ConversionFlags.Format44,        new DDS.PixelFormat(DDS.PixelFormatFlags.Luminance, 0,  8, 0x0f,       0x00,       0x00,       0xf0       ) ), // D3DFMT_A4L4 (uses DXGI 1.2 format)
#else
            // !DXGI_1_2_FORMATS
            new LegacyMap(PixelFormats.R8G8B8A8_UNORM, ConversionFlags.Expand
                | ConversionFlags.Format4444, DDSFormat.PixelFormat.A4R4G4B4), // D3DFMT_A4R4G4B4
            new LegacyMap(PixelFormats.R8G8B8A8_UNORM, ConversionFlags.Expand
                | ConversionFlags.NoAlpha
                | ConversionFlags.Format4444, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.Rgb, 0, 16, 0x0f00, 0x00f0, 0x000f, 0x0000)), // D3DFMT_X4R4G4B4
            new LegacyMap(PixelFormats.R8G8B8A8_UNORM, ConversionFlags.Expand
                | ConversionFlags.Format44, new DDSFormat.PixelFormat(DDSFormat.PixelFormatFlags.Luminance, 0, 8, 0x0f, 0x00, 0x00, 0xf0)), // D3DFMT_A4L4
#endif
        };


        // Note that many common DDS reader/writers (including D3DX) swap the
        // the RED/BLUE masks for 10:10:10:2 formats. We assumme
        // below that the 'backwards' header mask is being used since it is most
        // likely written by D3DX. The more robust solution is to use the 'DX10'
        // header extension and specify the Format.R10G10B10A2_UNorm format directly

        // We do not support the following legacy Direct3D 9 formats:
        //      BumpDuDv D3DFMT_V8U8, D3DFMT_Q8W8V8U8, D3DFMT_V16U16, D3DFMT_A2W10V10U10
        //      BumpLuminance D3DFMT_L6V5U5, D3DFMT_X8L8V8U8
        //      FourCC "UYVY" D3DFMT_UYVY
        //      FourCC "YUY2" D3DFMT_YUY2
        //      FourCC 117 D3DFMT_CxV8U8
        //      ZBuffer D3DFMT_D16_LOCKABLE
        //      FourCC 82 D3DFMT_D32F_LOCKABLE
        public static PixelFormats GetDXGIFormat(ref DDSFormat.PixelFormat pixelFormat, out ConversionFlags conversionFlags, DDSFlags flags = DDSFlags.None)
        {
            conversionFlags = ConversionFlags.None;

            int index = 0;
            for (index = 0; index < LegacyMaps.Length; ++index)
            {
                var entry = LegacyMaps[index];

                if ((pixelFormat.Flags & entry.PixelFormat.Flags) != 0)
                {
                    if ((entry.PixelFormat.Flags & DDSFormat.PixelFormatFlags.FourCC) != 0)
                    {
                        if (pixelFormat.FourCC == entry.PixelFormat.FourCC)
                            break;
                    }
                    else if ((entry.PixelFormat.Flags & DDSFormat.PixelFormatFlags.Pal8) != 0)
                    {
                        if (pixelFormat.RGBBitCount == entry.PixelFormat.RGBBitCount)
                            break;
                    }
                    else if (pixelFormat.RGBBitCount == entry.PixelFormat.RGBBitCount)
                    {
                        // RGB, RGBA, ALPHA, LUMINANCE
                        if (pixelFormat.RBitMask == entry.PixelFormat.RBitMask
                                && pixelFormat.GBitMask == entry.PixelFormat.GBitMask
                                && pixelFormat.BBitMask == entry.PixelFormat.BBitMask
                                && pixelFormat.ABitMask == entry.PixelFormat.ABitMask)
                            break;
                    }
                }
            }

            if (index >= LegacyMaps.Length)
                return PixelFormats.UNKNOWN;

            conversionFlags = LegacyMaps[index].ConversionFlags;
            var format = LegacyMaps[index].Format;

            if ((conversionFlags & ConversionFlags.Expand) != 0 && (flags & DDSFlags.NoLegacyExpansion) != 0)
                return PixelFormats.UNKNOWN;

            if ((format == PixelFormats.R10G10B10A2_UNORM) && (flags & DDSFlags.NoR10B10G10A2Fixup) != 0)
            {
                conversionFlags ^= ConversionFlags.Swizzle;
            }

            return format;
        }

        /// <summary>
        /// Flags used by <see cref="DDSHelper.LoadFromDDSMemory"/>.
        /// </summary>
        [Flags]
        public enum DDSFlags
        {
            None = 0x0,
            LegacyDword = 0x1, // Assume pitch is DWORD aligned instead of BYTE aligned (used by some legacy DDS files)
            NoLegacyExpansion = 0x2, // Do not implicitly convert legacy formats that result in larger pixel sizes (24 bpp, 3:3:2, A8L8, A4L4, P8, A8P8) 
            NoR10B10G10A2Fixup = 0x4, // Do not use work-around for long-standing D3DX DDS file format issue which reversed the 10:10:10:2 color order masks
            ForceRgb = 0x8, // Convert DXGI 1.1 BGR formats to Format.R8G8B8A8_UNorm to avoid use of optional WDDM 1.1 formats
            No16Bpp = 0x10, // Conversions avoid use of 565, 5551, and 4444 formats and instead expand to 8888 to avoid use of optional WDDM 1.2 formats
            CopyMemory = 0x20, // The content of the memory passed to the DDS Loader is copied to another internal buffer.
            ForceDX10Ext = 0x10000, // Always use the 'DX10' header extension for DDS writer (i.e. don't try to write DX9 compatible DDS files)
        };

    }
}
