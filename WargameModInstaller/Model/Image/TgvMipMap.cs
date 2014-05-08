using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Image
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Credits to enohka for this.
    /// See more at: http://github.com/enohka/moddingSuite
    /// </remarks>
    public class TgvMipMap
    {
        public TgvMipMap()
        {
        }

        public TgvMipMap(uint offset, uint length, uint mipSize, uint mipWidth, uint mipHeight)
        {
            this.Offset = offset;
            this.Length = length;
            this.MipSize = mipSize;
            this.MipWidth = mipWidth;
            this.MipHeight = mipHeight;
        }

        /// <summary>
        /// Gets or sets an offset of the MipMap from the begining of the TGV file.
        /// </summary>
        public uint Offset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a length of the byte content of the MipMap in the file (only content length).
        /// </summary>
        public uint Length
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a dimensional size of the MipMap.
        /// </summary>
        public uint MipSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a width of the MipMap image.
        /// </summary>
        public uint MipWidth
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a height of the MipMap image.
        /// </summary>
        public uint MipHeight
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a raw MipMap Image content.
        /// </summary>
        public byte[] Content
        {
            get;
            set;
        }

    }
}
