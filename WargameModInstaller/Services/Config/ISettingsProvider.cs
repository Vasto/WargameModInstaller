using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Config;
using WargameModInstaller.Model.Config;

namespace WargameModInstaller.Services.Config
{
    public interface ISettingsProvider
    {
        //bool AreGeneralSettingsAvailable(GeneralSettingsEntry entryType);
        GeneralSetting GetGeneralSettings(GeneralSettingEntryType entryType);
        //bool AreScreenSettingsAvailable(ScreenSettingsEntry entryType);
        ScreenSettings GetScreenSettings(ScreenSettingsEntryType entryType);
    }
}
