using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    /// <summary>
    /// Represents a group of installation commands which target common Edata file.
    /// </summary>
    public class SharedTargetCmdGroup : ICmdGroup
    {
        private readonly List<IInstallCmd> commands;

        public SharedTargetCmdGroup
            (IEnumerable<IInstallCmd> commands, 
            InstallEntityPath targetPath)
        {
            this.commands = new List<IInstallCmd>(commands);
            this.TargetPath = targetPath;
        }

        public SharedTargetCmdGroup(
            IEnumerable<IInstallCmd> commands, 
            InstallEntityPath targetPath,
            int priority)
        {
            this.commands = new List<IInstallCmd>(commands);
            this.TargetPath = targetPath;
            this.Priority = priority;
        }

        /// <summary>
        /// Gets or sets the execution priority of the current group.
        /// </summary>
        public int Priority
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the set of commands which belong to the current group.
        /// </summary>
        public IReadOnlyCollection<IInstallCmd> Commands
        {
            get 
            { 
                return commands; 
            }
        }

        /// <summary>
        /// Gets the path of the common target file for this commands group.
        /// </summary>
        public InstallEntityPath TargetPath
        {
            get;
            private set;
        }

    }

}
