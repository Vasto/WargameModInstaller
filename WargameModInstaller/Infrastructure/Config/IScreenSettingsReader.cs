using System;
using WargameModInstaller.Model.Config;

namespace WargameModInstaller.Infrastructure.Config
{
    public interface IScreenSettingsReader
    {
        ScreenSettings Read(String file, ScreenSettingsEntryType entry);
    }
}
