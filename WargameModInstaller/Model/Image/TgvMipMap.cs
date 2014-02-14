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

        public TgvMipMap(uint offset, uint size, ushort mipWidth)
        {
            this.Offset = offset;
            this.Size = size;
            this.MipWidth = mipWidth;
        }

        public uint Offset
        {
            get;
            set;
        }

        public uint Size
        {
            get;
            set;
        }

        public int MipWidth
        {
            get;
            set;
        }

        public byte[] Content
        {
            get;
            set;
        }

    }
}
