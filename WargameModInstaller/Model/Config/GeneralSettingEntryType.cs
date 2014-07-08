using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Config
{
    public class GeneralSettingEntryType : SettingEntryType
    {
        public static readonly GeneralSettingEntryType InstallationBackup = new GeneralSettingEntryType(1, "InstallationBackup");
        public static readonly GeneralSettingEntryType ModName = new GeneralSettingEntryType(2, "ModName");
        public static readonly GeneralSettingEntryType CriticalCommands = new GeneralSettingEntryType(3, "CriticalCommands");
        public static readonly GeneralSettingEntryType WargameVersion = new GeneralSettingEntryType(4, "WargameVersion");
        public static readonly GeneralSettingEntryType AutoInstall = new GeneralSettingEntryType(5, "AutoInstall");

        protected GeneralSettingEntryType(int value, String name)
            : base(value, name)
        {

        }
    }
}
