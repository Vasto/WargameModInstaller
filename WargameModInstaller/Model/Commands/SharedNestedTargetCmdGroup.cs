using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    //To do: W obecnym kontekście nazw, to wcale nie sugeruje, że ta klasa grupuje tylko te kommendy
    // które odnoszą się do wspólnego wielokrotnie zagniezdzonego kontenera, noi nazwa jakas tak długa (ale trudno o krótszą)...

    //CZy to nie mołgo by dziedziczyć z SharedTargetCmdGroup?

    /// <summary>
    /// Represents a group of installation commands which target the same hierarchy of package files.
    /// </summary>
    public class SharedNestedTargetCmdGroup : ICmdGroup
    {
        private readonly List<IInstallCmd> commands;

        public SharedNestedTargetCmdGroup(
            IEnumerable<IInstallCmd> commands, 
            InstallEntityPath targetPath,
            ContentPath nestedTargetPath)
        {
            this.commands = new List<IInstallCmd>(commands);
            this.TargetPath = targetPath;
            this.NestedTargetPath = nestedTargetPath;
        }

        public SharedNestedTargetCmdGroup(
            IEnumerable<IInstallCmd> commands,
            InstallEntityPath targetPath,
            ContentPath nestedTargetPath,
            int priority)
        {
            this.commands = new List<IInstallCmd>(commands);
            this.TargetPath = targetPath;
            this.NestedTargetPath = nestedTargetPath;
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
        /// Gets the set of commands which belong to current group.
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

        /// <summary>
        /// Gets the path of the common nested target file for this commands group.
        /// </summary>
        public ContentPath NestedTargetPath
        {
            get;
            private set;
        }

    }
}
