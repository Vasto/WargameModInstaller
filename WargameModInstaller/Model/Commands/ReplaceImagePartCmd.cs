using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    public class ReplaceImagePartCmd : IInstallCmd, IHasSource, IHasTarget, IHasTargetContent
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
        /// Gets or sets a command execution priority.
        /// Commands with a higher priority are executed sooner.
        /// </summary>
        public int Priority
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a path to a image file which has to be used as a replacer
        /// </summary>
        public InstallEntityPath SourcePath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a path to the dat file which holds the image which part has to be replaced.
        /// </summary>
        public InstallEntityPath TargetPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a path to a content inside the dat file.
        /// </summary>
        public ContentPath TargetContentPath
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
        /// Gets or sets an X coordinate of the point where the replacement starts.
        /// </summary>
        public int? XPosition
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a Y coordinate of the point where the replacement starts.
        /// </summary>
        public int? YPosition
        {
            get;
            set;
        }

        public String GetExecutionMessage()
        {
            return String.Format(WargameModInstaller.Properties.Resources.Copying + " {0}...",
                SourcePath);
        }


    }

}
