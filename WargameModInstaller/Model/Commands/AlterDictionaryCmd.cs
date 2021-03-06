﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class AlterDictionaryCmd : InstallCmdBase, IHasTarget, IHasNestedTarget
    {
        /// <summary>
        /// 
        /// </summary>
        public AlterDictionaryCmd()
        {
            this.AddedEntries = new List<KeyValuePair<String, String>>();
            this.RemovedEntries = new List<String>();
            this.RenamedEntries = new List<KeyValuePair<String, String>>();
        }

        /// <summary>
        /// Gets or sets a set of hash value pairs of entries which ahve to be added.
        /// </summary>
        public IEnumerable<KeyValuePair<String, String>> AddedEntries
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a set of hashes of entries which have to be removed.
        /// </summary>
        public IEnumerable<String> RemovedEntries
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a set containing a pairs of entries hashes and their new names.
        /// </summary>
        public IEnumerable<KeyValuePair<String, String>> RenamedEntries
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a path to the dat file which holds the image to replace
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

        protected override String GetCommandsName()
        {
            return "AlterDictionaryCommand";
        }

        protected override String GetExecutionMessage()
        {
            return String.Format(Properties.Resources.AlteringDictionary + " {0}...",
                NestedTargetPath);
        }

    }
}
