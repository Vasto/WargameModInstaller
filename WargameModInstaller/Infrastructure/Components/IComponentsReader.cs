using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Components;

namespace WargameModInstaller.Infrastructure.Components
{
    public interface IComponentsReader
    {
        IEnumerable<Component> ReadAll(String filePath);
    }
}
