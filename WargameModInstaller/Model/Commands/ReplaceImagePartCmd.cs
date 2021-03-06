﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    public class ReplaceImagePartCmd : InstallCmdBase, IHasSource, IHasTarget, IHasNestedTarget
    {
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
        public ContentPath NestedTargetPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the information whether the command requires to work an orginal content of target. 
        /// </summary>
        public bool UsesNestedTargetContent
        {
            get { return true; }
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

        /// <summary>
        /// Gets or sets an information wheather a MipMaps of the image 
        /// should be taken into consdieration during the replacement process.
        /// </summary>
        public bool UseMipMaps
        {
            get;
            set;
        }

        protected override String GetCommandsName()
        {
            return "ReplaceImagePartCommand";
        }

        protected override String GetExecutionMessage()
        {
            return String.Format(Properties.Resources.Copying + " {0}...",
                SourcePath);
        }

    }

}
