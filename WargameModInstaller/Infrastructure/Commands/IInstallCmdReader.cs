using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Config;
using WargameModInstaller.Model.Commands;

namespace WargameModInstaller.Infrastructure.Commands
{
    /// <summary>
    /// Represents the install commands reader.
    /// </summary>
    public interface IInstallCmdReader
    {
        IEnumerable<IInstallCmd> Read(String filePath, CmdEntryType entryType);
        IEnumerable<IInstallCmd> ReadAll(String filePath);
        IEnumerable<ICmdGroup> ReadGroups(String filePath);
    }
}
