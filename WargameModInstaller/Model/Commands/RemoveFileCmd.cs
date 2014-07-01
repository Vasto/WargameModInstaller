using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.Model.Commands
{
    //IHasSource or IHasTarget?

    public class RemoveFileCmd : InstallCmdBase, IHasSource
    {
        /// <summary>
        /// Gets or sets a file path which has to be removed.
        /// </summary>
        public InstallEntityPath SourcePath
        {
            get;
            set;
        }

        protected override String GetCommandsName()
        {
            return "RemoveFileCommand";
        }

        protected override String GetExecutionMessage()
        {
            return String.Format(Properties.Resources.Removing + " {0}...",
                System.IO.Path.GetFileName(SourcePath));
        }

    }

}
