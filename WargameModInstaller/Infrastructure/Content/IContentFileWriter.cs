using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Content
{
    public interface IContentFileWriter
    {
        void Write(String contentFilePath, byte[] content);
    }
}
