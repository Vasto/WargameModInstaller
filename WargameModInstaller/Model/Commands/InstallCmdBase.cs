using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Commands
{
    public abstract class InstallCmdBase : IInstallCmd
    {
        /// <summary>
        /// Gets or sets a command ID.
        /// </summary>
        public int Id
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
        /// Gets the message which contains a descriptive text of command's execution.
        /// </summary>
        /// <returns></returns>
        public abstract String GetExecutionMessage();

    }
}
