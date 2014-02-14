using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Config
{
    public class CmdEntryType : WMIEntryType
    {
        public static readonly CmdEntryType CopyGameFile = new CmdEntryType("CopyGameFile");
        public static readonly CmdEntryType CopyModFile = new CmdEntryType("CopyModFile");
        public static readonly CmdEntryType RemoveFile = new CmdEntryType("RemoveFile");
        public static readonly CmdEntryType ReplaceImage = new CmdEntryType("ReplaceImage");
        public static readonly CmdEntryType ReplaceImagePart = new CmdEntryType("ReplaceImagePart");
        public static readonly CmdEntryType ReplaceImageTile = new CmdEntryType("ReplaceImageTile");

        protected CmdEntryType(String entryName)
            : base(entryName)
        {

        }

    }

}
