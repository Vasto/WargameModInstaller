using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.Model.Commands
{
    public class CopyGameFileCmd : IInstallCmd, IHasSource, IHasTarget
    {
        public CopyGameFileCmd()
        {

        }

        /// <summary>
        /// Gets or sets a command ID.
        /// </summary>
        public int Id
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a command execution priority.
        /// Commands with a higher priority are executed sooner.
        /// </summary>
        public int Priority
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a path to the file which has to be copied
        /// </summary>
        public WMIPath SourcePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a path to a location where the file has to be copied.
        /// </summary>
        public WMIPath TargetPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an information wheather a command is critical.
        /// If the critical command fails, whole installation fails.
        /// </summary>
        public bool IsCritical
        {
            get;
            set;
        }

        public String GetExecutionMessage()
        {
            return String.Format(WargameModInstaller.Properties.Resources.Backuping + " {0}...",
                System.IO.Path.GetFileName(SourcePath));
        }

    }

}
