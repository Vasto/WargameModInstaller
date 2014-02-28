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
        GeneralSetting GetGeneralSettings(GeneralSettingEntryType entryType);
        ScreenSettings GetScreenSettings(ScreenSettingsEntryType entryType);
    }
}
