﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Config
{
    public class CmdEntryType : WMIEntryType
    {
        public static readonly CmdEntryType CopyGameFile = new CmdEntryType(1, "CopyGameFile");
        public static readonly CmdEntryType CopyModFile = new CmdEntryType(2, "CopyModFile");
        public static readonly CmdEntryType RemoveFile = new CmdEntryType(3, "RemoveFile");
        public static readonly CmdEntryType ReplaceImage = new CmdEntryType(4, "ReplaceImage");
        public static readonly CmdEntryType ReplaceImagePart = new CmdEntryType(5, "ReplaceImagePart");
        public static readonly CmdEntryType ReplaceImageTile = new CmdEntryType(6, "ReplaceImageTile");

        protected CmdEntryType(int value, String name)
            : base(value, name)
        {

        }

    }

}