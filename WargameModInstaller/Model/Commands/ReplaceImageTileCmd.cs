using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.Model.Commands
{
    public class ReplaceImageTileCmd : InstallCmdBase, IHasSource, IHasTarget, IHasNestedTarget
    {
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
        public ContentPath NestedTargetPath
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
        /// Gets or sets a size of the replacement cell.
        /// </summary>
        public int? TileSize
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an information wheather a MipMaps of the image 
        /// should be taken into consdieration during the replacement process.
        /// </summary>
        public bool UseMipMaps
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
            return String.Format(Properties.Resources.Copying + " {0}...",
                SourcePath);
        }


    }

}
