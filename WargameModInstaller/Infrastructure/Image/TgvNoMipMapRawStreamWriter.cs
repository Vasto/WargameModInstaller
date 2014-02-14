using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Utilities;
using WargameModInstaller.Utilities.Compression;

namespace WargameModInstaller.Infrastructure.Image
{
    /// <summary>
    /// Writes the Tgv file in binary form to the provided Stream without any additional MipMaps. 
    /// </summary>
    public class TgvNoMipMapRawStreamWriter : TgvRawStreamWriter
    {
        public TgvNoMipMapRawStreamWriter(Stream stream) : base(stream)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        /// <remarks>
        /// Method based on enohka's code.
        /// See more at: http://github.com/enohka/moddingSuite
        /// </remarks>
        public override void Write(TgvImage file)
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

            base.Write(noMipMapsFile);
        }

    }
}
