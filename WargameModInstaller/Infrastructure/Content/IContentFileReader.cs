using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Content
{
    public interface IContentFileReader
    {
        byte[] Read(String contentFilePath);
    }
}
