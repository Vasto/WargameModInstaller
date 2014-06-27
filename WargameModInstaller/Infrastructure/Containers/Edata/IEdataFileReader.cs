using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WargameModInstaller.Model.Containers.Edata;
using WargameModInstaller.Model;
using System.Threading;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    public interface IEdataFileReader
    {
        EdataFile Read(String edataFilePath, bool loadContent);
        EdataFile Read(String edataFilePath, bool loadContent, CancellationToken token);
        byte[] ReadContent(EdataContentFile file);
        void LoadContent(EdataContentFile file);
        void LoadContent(IEnumerable<EdataContentFile> files);
    }
}
