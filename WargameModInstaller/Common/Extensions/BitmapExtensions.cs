using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WargameModInstaller.Common.Extensions
{
    public static class BitmapExtensions
    {
        /// <summary>
        /// Converts a System.Drawing.Bitmap to a BitmapImage.
        /// </summary>
        /// <param name="obj"></param>
        public static BitmapImage ToBitmapImage(this Bitmap obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                obj.Save(memoryStream, ImageFormat.Bmp);
                memoryStream.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        /// <summary>
        /// Converts a System.Drawing.Bitmap to a BitmapSource.
        /// </summary>
        /// <param name="obj"></param>
        public static BitmapSource ToBitmapSource(this Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ip,
                    IntPtr.Zero, 
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

    }
}
