using System;
using WargameModInstaller.Model.Config;

namespace WargameModInstaller.Infrastructure.Config
{
    public interface IGeneralSettingReader
    {
        GeneralSetting Read(String file, GeneralSettingEntryType entryType);
    }
}
