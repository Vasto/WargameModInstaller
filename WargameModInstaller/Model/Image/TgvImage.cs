using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities.Image.DDS;

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
            //this.Offsets = new List<uint>();
            //this.Sizes = new List<uint>();
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

        //public List<uint> Offsets
        //{
        //    get;
        //    set;
        //}

        //public List<uint> Sizes
        //{
        //    get;
        //    set;
        //}

        public string PixelFormatString
        {
            get;
            set;
        }

        public IList<TgvMipMap> MipMaps
        {
            get;
            set;
        }

        //Jakby by³a potrzeba to temu mo¿na przekazywaæ strategie.
        public virtual byte[] ComputeContentChecksum()
        {
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                foreach (var mip in MipMaps)
                {
                    ms.Write(mip.Content, 0, mip.Content.Length);
                }

                var checksum = System.Security.Cryptography.MD5.Create().ComputeHash(ms.ToArray());
                return checksum;
            }
        }

    }
}
