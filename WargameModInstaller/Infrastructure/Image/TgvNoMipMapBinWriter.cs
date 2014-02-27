using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Utilities.Compression;

namespace WargameModInstaller.Infrastructure.Image
{
    /// <summary>
    /// Writes a Tgv file to the byte array without any additional MipMaps. 
    /// </summary>
    public class TgvNoMipMapBinWriter : TgvBinWriter
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public override byte[] Write(TgvImage file)
        {
            TgvImage noMipMapsFile = new TgvImage();
            noMipMapsFile.PixelFormatString = file.PixelFormatString;
            noMipMapsFile.Format = file.Format;
            noMipMapsFile.Height = file.Height;
            noMipMapsFile.Width = file.Width;
            noMipMapsFile.ImageHeight = file.ImageHeight;
            noMipMapsFile.ImageWidth = file.ImageWidth;
            noMipMapsFile.IsCompressed = file.IsCompressed;
            noMipMapsFile.Offsets = file.Offsets;
            noMipMapsFile.Sizes = file.Sizes;
            noMipMapsFile.SourceChecksum = file.SourceChecksum;
            noMipMapsFile.Version = file.Version;
            //Use only the main content MipMap
            noMipMapsFile.MipMaps.Add(file.MipMaps.ToArray().OrderBy(x => x.Size).Last());
            noMipMapsFile.MipMapCount = 1;

            return base.Write(noMipMapsFile);
        }

    }
}
