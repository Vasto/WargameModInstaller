using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WargameModInstaller.Services.Install
{
    public class WNInstallDirProvider : InstallDirProviderBase
    {
        protected override String GetWargameName()
        {
            return "WARNO.exe";
        }

        protected override String GetWargameFolderName()
        {
            return "WARNO";
        }

        protected override IEnumerable<String> GetPotentialInstallDirectoryPaths()
        {
            var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesName = (new DirectoryInfo(programFilesPath)).Name;

            var results = new List<String>();
            var drives = from d in DriveInfo.GetDrives()
                         where d.DriveType == DriveType.Fixed
                         select d.Name;
            foreach (var drive in drives)
            {
                results.Add(Path.Combine(drive, programFilesName, @"Steam\SteamApps\common\WARNO\"));
                results.Add(Path.Combine(drive, @"SteamLibrary\steamapps\common\WARNO\"));
            }

            return results;
        }

        protected override IEnumerable<String> GetPotentialInstallRegistryKeys()
        {
            var results = new List<String>();
            results.Add(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 1611600");
            results.Add(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 1611600");

            return results;
        }
    }
}
