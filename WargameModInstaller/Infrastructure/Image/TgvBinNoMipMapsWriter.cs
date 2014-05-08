using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Common.Utilities.Compression;

namespace WargameModInstaller.Infrastructure.Image
{
    /// <summary>
    /// A writer which can write a TGV image to a byte form. 
    /// This writer discards any additional MipMaps from the writing process.
    /// </summary>
    public class TgvBinNoMipMapsWriter : TgvBinWriter
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
            noMipMapsFile.SourceChecksum = file.SourceChecksum;
            noMipMapsFile.Version = file.Version;
            noMipMapsFile.MipMaps.Add(file.MipMaps.ToArray().OrderBy(x => x.Length).Last());
            noMipMapsFile.MipMapCount = 1;

            return base.Write(noMipMapsFile);
        }

    }
}
