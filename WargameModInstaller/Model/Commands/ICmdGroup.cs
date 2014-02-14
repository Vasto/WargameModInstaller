using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Commands
{
    public interface ICmdGroup
    {
        int Priority { get; set; }
        IEnumerable<IInstallCmd> Commands { get; }
        int CommandsCount { get; }
    }
}
