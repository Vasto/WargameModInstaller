using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    public class BackupProfileCmd : InstallCmdBase, IHasTarget
    {
        /// <summary>
        /// Gets or sets a file path where the profile has to be backuped.
        /// </summary>
        public InstallEntityPath TargetPath
        {
            get;
            set;
        }

        protected override String GetCommandsName()
        {
            return "BackupProfileCmd";
        }

        protected override String GetExecutionMessage()
        {
            return String.Format(Properties.Resources.Backuping + " {0}...",
                "Profile");
        }

    }
}
