using System;

namespace WargameModInstaller.Services.Install
{
    /// <summary>
    /// 
    /// </summary>
    public interface IWargameInstallDirService
    {
        bool IsCorrectInstallDirectory(String installDirPath);
        String TryGetInstallDirectory();
    }
}
