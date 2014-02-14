using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Config
{
    public class ScreenSettingsEntryType : SettingEntryType
    {
        public static readonly ScreenSettingsEntryType WelcomeScreen = new ScreenSettingsEntryType("WelcomeScreen");
        public static readonly ScreenSettingsEntryType LocationScreen = new ScreenSettingsEntryType("LocationScreen");
        public static readonly ScreenSettingsEntryType ProgressScreen = new ScreenSettingsEntryType("ProgressScreen");
        public static readonly ScreenSettingsEntryType InstallCompletedScreen = new ScreenSettingsEntryType("InstallCompletedScreen");
        public static readonly ScreenSettingsEntryType InstallFailedScreen = new ScreenSettingsEntryType("InstallFailedScreen");
        public static readonly ScreenSettingsEntryType InstallCanceledScreen = new ScreenSettingsEntryType("InstallCanceledScreen");

        protected ScreenSettingsEntryType(String entryName)
            : base(entryName)
        {

        }
    }
}
