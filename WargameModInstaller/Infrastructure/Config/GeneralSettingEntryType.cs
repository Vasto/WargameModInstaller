using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Config
{
    public class GeneralSettingEntryType : SettingEntryType
    {
        public static readonly GeneralSettingEntryType InstallationBackup = new GeneralSettingEntryType("InstallationBackup");
        public static readonly GeneralSettingEntryType ModName = new GeneralSettingEntryType("ModName");
        public static readonly GeneralSettingEntryType CriticalCommands = new GeneralSettingEntryType("CriticalCommands");

        protected GeneralSettingEntryType(String entryName)
            : base(entryName)
        {

        }
    }
}
