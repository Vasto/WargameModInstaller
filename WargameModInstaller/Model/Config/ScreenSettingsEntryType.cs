using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Config
{
    public class ScreenSettingsEntryType : SettingEntryType
    {
        public static readonly ScreenSettingsEntryType WelcomeScreen = new ScreenSettingsEntryType(1, "WelcomeScreen");
        public static readonly ScreenSettingsEntryType LocationScreen = new ScreenSettingsEntryType(2, "LocationScreen");
        public static readonly ScreenSettingsEntryType ProgressScreen = new ScreenSettingsEntryType(3, "ProgressScreen");
        public static readonly ScreenSettingsEntryType InstallCompletedScreen = new ScreenSettingsEntryType(4, "InstallCompletedScreen");
        public static readonly ScreenSettingsEntryType InstallFailedScreen = new ScreenSettingsEntryType(5, "InstallFailedScreen");
        public static readonly ScreenSettingsEntryType InstallCanceledScreen = new ScreenSettingsEntryType(7, "InstallCanceledScreen");

        protected ScreenSettingsEntryType(int value, String entryName)
            : base(value, entryName)
        {

        }
    }
}
