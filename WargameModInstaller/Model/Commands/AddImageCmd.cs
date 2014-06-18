using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    public class AddImageCmd : InstallCmdBase, IHasSource, IHasTarget, IHasNestedTarget
    {
        /// <summary>
        /// Gets or sets a path to a file file which has to be added to the target container file.
        /// </summary>
        public InstallEntityPath SourcePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a path to a conteiner file, where the source file has to be added.
        /// </summary>
        public InstallEntityPath TargetPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a path where the source file has to be added in the container target file.
        /// </summary>
        public ContentPath NestedTargetPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the message which contains a descriptive text of command's execution.
        /// </summary>
        /// <returns></returns>
        public override String GetExecutionMessage()
        {
            return String.Format(Properties.Resources.Adding + " {0}...",
                SourcePath);
        }

    }
}
