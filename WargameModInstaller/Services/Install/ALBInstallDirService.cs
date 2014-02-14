using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Services.Install
{
    /// <summary>
    /// 
    /// </summary>
    public class ALBInstallDirService : IWargameInstallDirService
    {
        public bool IsCorrectInstallDirectory(String installDirPath)
        {
            if (!Directory.Exists(installDirPath))
            {
                return false;
            }

            var albExecutableName = "WarGame2.exe";
            var albFolderName = "Wargame Airland Battle";
            if (installDirPath.Contains(albFolderName))
            {
                var albDirectoryPath = installDirPath.Substring(0, installDirPath.IndexOf(albFolderName) + albFolderName.Length);
                var exeFiles = Directory.GetFiles(albDirectoryPath, "*.exe", SearchOption.AllDirectories);
                if (exeFiles.Any(file => file.Contains(albExecutableName)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String TryGetInstallDirectory()
        {
            var installDirsPaths = GetPotentialInstallDirectoriesPaths();
            foreach (var dirPath in installDirsPaths)
            {
                if (Directory.Exists(dirPath))
                {
                    return dirPath;
                }
            }

            var installRegistryKeys = GetPotentialInstallRegistryKeys();
            foreach (var key in installRegistryKeys)
            {
                var installLocation = Registry.GetValue(key, "InstallLocation", null) as String;
                if (installLocation != null)
                {
                    if (Directory.Exists(installLocation))
                    {
                        return installLocation;
                    }
                }
            }

            return String.Empty;
        }

        private IEnumerable<String> GetPotentialInstallDirectoriesPaths()
        {
            var programFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilesName = (new DirectoryInfo(programFilesPath)).Name;

            var results = new List<String>();
            var drives = from d in DriveInfo.GetDrives()
                         where d.DriveType == DriveType.Fixed
                         select d.Name;
            foreach (var drive in drives)
            {
                results.Add(Path.Combine(drive, programFilesName, @"Steam\SteamApps\common\Wargame Airland Battle\"));
            }

            return results;
        }

        private IEnumerable<String> GetPotentialInstallRegistryKeys()
        {
            var results = new List<String>();
            results.Add(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 222750");
            results.Add(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 222750");

            return results;
        }


    }
}
