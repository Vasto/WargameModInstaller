using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Content
{
    public class ContentFileWriter : IContentFileWriter
    {
        public void Write(String contentFilePath, byte[] content)
        {
            File.WriteAllBytes(contentFilePath, content);
        }

    }
}
