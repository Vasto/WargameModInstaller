using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Utilities.Image.DDS;

namespace WargameModInstaller.Model.Image
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Credits to enohka for this.
    /// See more at: http://github.com/enohka/moddingSuite
    /// </remarks>
    public class TgvImage
    {
        public TgvImage()
        {
            this.Offsets = new List<uint>();
            this.Sizes = new List<uint>();
            this.MipMaps = new List<TgvMipMap>();
        }

        public uint Version
        {
            get;
            set;
        }

        public bool IsCompressed
        {
            get;
            set;
        }

        public uint Width
        {
            get;
            set;
        }

        public uint Height
        {
            get;
            set;
        }

        public uint ImageWidth
        {
            get;
            set;
        }

        public uint ImageHeight
        {
            get;
            set;
        }

        public ushort MipMapCount
        {
            get;
            set;
        }

        public PixelFormats Format
        {
            get;
            set;
        }

        public byte[] SourceChecksum
        {
            get;
            set;
        }

        public List<uint> Offsets
        {
            get;
            set;
        }

        public List<uint> Sizes
        {
            get;
            set;
        }

        public string PixelFormatString
        {
            get;
            set;
        }

        public List<TgvMipMap> MipMaps
        {
            get;
            private set;
        }

    }
}
