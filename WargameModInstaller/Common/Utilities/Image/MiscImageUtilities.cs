using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.Common.Utilities.Image
{
    public static class MiscImageUtilities
    {
        public static BitmapSource LoadBitmap(ResourcePath path)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }

            if (path.PathType == ResourcePathType.LocalAbsolute)
            {
                var fullPath = path.Value;
                if (File.Exists(fullPath))
                {
                    return new BitmapImage(new Uri(fullPath));
                }
                else
                {
                    throw new IOException("File with the given path doesn't exist");
                }
            }
            else if (path.PathType == ResourcePathType.LocalRelative)
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.Value);
                if (File.Exists(fullPath))
                {
                    return new BitmapImage(new Uri(fullPath));
                }
                else
                {
                    throw new IOException("File with the given path doesn't exist");
                }
            }
            else if (path.PathType == ResourcePathType.Embedded)
            {
                var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                if (resourceNames.Contains(path.Value))
                {
                    return (new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(path.Value))).ToBitmapSource();
                }
                else
                {
                    throw new IOException("Specified resource doesn't exist");
                }
            }
            else
            {
                throw new ArgumentException("Unsupported path type", "path");
            }
        
        }

    }

}
