using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Content
{
    public class ContentFileReader : IContentFileReader
    {
        public byte[] Read(String contentFilePath)
        {
            return File.ReadAllBytes(contentFilePath);
        }

    }
}
