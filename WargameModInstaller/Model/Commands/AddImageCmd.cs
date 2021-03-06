﻿using System;
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
        /// Gets the information whether the command requires to work an orginal content of target. 
        /// </summary>
        public bool UsesNestedTargetContent
        {
            get { return false; }
        }

        /// <summary>
        /// Gets or sets the information whether the command should overwrite a content 
        /// of the target content file when the command cannot add that file because it already exist.
        /// </summary>
        public bool OverwriteIfExist
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an information wheather a MipMaps of the image 
        /// should be taken into consdieration during the image addition process.
        /// </summary>
        public bool UseMipMaps
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets an information wheather the content 
        /// of the image has to be compressed during the image addition procces.
        /// </summary>
        public bool UseCompression
        {
            get;
            set;
        }

        public String Checksum
        {
            get;
            set;
        }

        protected override String GetCommandsName()
        {
            return "AddImageCommand";
        }

        protected override String GetExecutionMessage()
        {
            return String.Format(Properties.Resources.Adding + " {0}...",
                SourcePath);
        }

    }
}
