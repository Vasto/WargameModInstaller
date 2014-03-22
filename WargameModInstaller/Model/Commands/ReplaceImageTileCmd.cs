using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.Model.Commands
{
    public class ReplaceImageTileCmd : IInstallCmd, IHasSource, IHasTarget, IHasTargetContent
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
        /// Gets or sets a path to a image file which has to be used as a replacer.
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
        /// Gets or sets a zero based column index, which defines a column where the replacement area lays.
        /// </summary>
        public int? Column
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a zero based row index, which defines a row where the replacement area lays.
        /// </summary>
        public int? Row
        {
            get;
            set;
        }

        /// <summary>
        /// Size of a replacement cell.
        /// </summary>
        public int? TileSize
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
