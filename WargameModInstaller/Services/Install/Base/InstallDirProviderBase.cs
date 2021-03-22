using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WargameModInstaller.Services.Install
{
    public abstract class InstallDirProviderBase : IWargameInstallDirService
    {
        public virtual bool IsCorrectInstallDirectory(String installDirPath)
        {
            if (!Directory.Exists(installDirPath))
            {
                return false;
            }

            var rdExecutableName = GetWargameName();
            var rdFolderName = GetWargameFolderName();
            String epicRdFolderName = Regex.Replace(rdFolderName, @"\s+", ""); //Folder name using the Epic launcher is WargameRedDragon
            if (installDirPath.Contains(rdFolderName))
            {
                var rdDirectoryPath = installDirPath.Substring(0, installDirPath.IndexOf(rdFolderName) + rdFolderName.Length);
                var exeFiles = Directory.GetFiles(rdDirectoryPath, "*.exe", SearchOption.AllDirectories);
                if (exeFiles.Any(file => file.Contains(rdExecutableName)))
                {
                    return true;
                }
            }
            else if (installDirPath.Contains(epicRdFolderName))
            {
                var rdDirectoryPath = installDirPath.Substring(0, installDirPath.IndexOf(epicRdFolderName) + epicRdFolderName.Length);
                var exeFiles = Directory.GetFiles(rdDirectoryPath, "*.exe", SearchOption.AllDirectories);
                if (exeFiles.Any(file => file.Contains(rdExecutableName)))
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
        public virtual String TryGetInstallDirectory()
        {
            var installDirsPaths = GetPotentialInstallDirectoryPaths();
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

            //Checking to see if installer shares directory or is in a subdirectory of the Wargame install location
            var currentFolder = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            String steamName = GetWargameFolderName();
            int steamInstallIndex = currentFolder.IndexOf(steamName);
            if (steamInstallIndex >= 0)
            {
                String gameFolderPath = currentFolder.Remove(steamInstallIndex + steamName.Length);
                if (IsCorrectInstallDirectory(gameFolderPath))
                {
                    return gameFolderPath;
                }
            }
            String epicName = Regex.Replace(steamName, @"\s+", ""); //Folder name using the Epic launcher is WargameRedDragon
            int epicInstallIndex = currentFolder.IndexOf(epicName);
            if (epicInstallIndex >= 0)
            {
                String gameFolderPath = currentFolder.Remove(epicInstallIndex + epicName.Length);
                if (IsCorrectInstallDirectory(gameFolderPath))
                {
                    return gameFolderPath;
                }
            }
            return String.Empty;
        }

        protected abstract String GetWargameName();

        protected abstract String GetWargameFolderName();

        protected abstract IEnumerable<String> GetPotentialInstallDirectoryPaths();

        protected abstract IEnumerable<String> GetPotentialInstallRegistryKeys();

    }
}
