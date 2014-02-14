using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Model;

namespace WargameModInstaller.Infrastructure.Edata
{
    public interface IEdataReader
    {
        EdataFile ReadAll(String edataFilePath, bool loadContent);
        //EdataFile ReadWithoutContent(String edataFilePath);
        void LoadContent(EdataContentFile file);
        void LoadContent(IEnumerable<EdataContentFile> files);
    }
}
