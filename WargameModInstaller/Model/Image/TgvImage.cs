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

        /// <summary>
        /// This is unaffected by the content of the image,
        /// rather it seems this is related to some of the image data, ie usage
        /// of alpha, usage of mipMaps (not the number of mipMaps), or the pixel format of image 
        /// (though the compression type DXT1 or DXT5 seems to have no effect)... 
        /// Though, it seems that there are not very common exceptions from those...
        /// Nonetheless values not matching game's pattern of this, don't seem to have any negative impact on stability...
        /// </summary>
        public byte[] SourceChecksum
        {
            get;
            set;
        }

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
        //Wygl¹da na to, ¿e ta checksuma jest zael¿na od formatu obrazka a nie do zawartosci, sæie¿ki czy nawet jego wymiarów.
        public virtual byte[] ComputeContentChecksum()
        {
            //This is pretty much far from giving any correct output...
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                int mipMaps = Convert.ToInt32(MipMapCount > 1);
                byte[] buffer = BitConverter.GetBytes(mipMaps);
                ms.Write(buffer, 0, buffer.Length);

                buffer = Encoding.ASCII.GetBytes(PixelFormatString);
                ms.Write(buffer, 0, buffer.Length);

                var checksum = System.Security.Cryptography.MD5.Create().ComputeHash(ms.ToArray());

                return checksum;
            }
        }

    }
}
