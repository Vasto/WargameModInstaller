using System;
using WargameModInstaller.Infrastructure.Config;

namespace WargameModInstaller.Model.Config
{
    public interface ISettingsFactory
    {
        object CreateSettings(SettingEntryType settingType);
        T CreateSettings<T>(SettingEntryType settingType) where T : class;
    }
}
