using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Services.Install
{
    public class RDInstallDirProvider : InstallDirProviderBase
    {
        protected override String GetWargameName()
        {
            return "WarGame3.exe";
        }

        protected override String GetWargameFolderName()
        {
            return "Wargame Red Dragon";
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
                results.Add(Path.Combine(drive, programFilesName, @"Steam\SteamApps\common\Wargame Red Dragon\"));
                results.Add(Path.Combine(drive, programFilesName, @"Epic Games\WargameRedDragon\"));
            }

            return results;
        }

        protected override IEnumerable<String> GetPotentialInstallRegistryKeys()
        {
            var results = new List<String>();
            results.Add(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 251060");
            results.Add(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 251060");

            return results;
        }

    }
}
