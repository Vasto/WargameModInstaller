using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.Model.Commands
{
    public class CopyGameFileCmd : InstallCmdBase, IHasSource, IHasTarget
    {
        /// <summary>
        /// Gets or sets a path to the file which has to be copied
        /// </summary>
        public InstallEntityPath SourcePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a path to a location where the file has to be copied.
        /// </summary>
        public InstallEntityPath TargetPath
        {
            get;
            set;
        }

        protected override String GetCommandsName()
        {
            return "CopyGameFileCmd";
        }

        protected override String GetExecutionMessage()
        {
            return String.Format(Properties.Resources.Backuping + " {0}...",
                System.IO.Path.GetFileName(SourcePath));
        }

    }

}
