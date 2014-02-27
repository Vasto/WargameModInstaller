using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Config
{
    public class GeneralSettingEntryType : SettingEntryType
    {
        public static readonly GeneralSettingEntryType InstallationBackup = new GeneralSettingEntryType(1, "InstallationBackup");
        public static readonly GeneralSettingEntryType ModName = new GeneralSettingEntryType(2, "ModName");
        public static readonly GeneralSettingEntryType CriticalCommands = new GeneralSettingEntryType(3, "CriticalCommands");

        protected GeneralSettingEntryType(int value, String name)
            : base(value, name)
        {

        }
    }
}
