using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    public class RestoreProfileCmd : InstallCmdBase, IHasSource
    {
        /// <summary>
        /// Gets or sets a path of backuped profile to restore.
        /// </summary>
        public InstallEntityPath SourcePath
        {
            get;
            set;
        }

        protected override String GetCommandsName()
        {
            return "RestoreProfileCmd";
        }

        protected override String GetExecutionMessage()
        {
            return String.Format(Properties.Resources.Restoring + " {0}...",
                "Profile");
        }
    }
}
