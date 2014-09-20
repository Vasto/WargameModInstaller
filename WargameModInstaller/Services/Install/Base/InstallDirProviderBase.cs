using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            if (installDirPath.Contains(rdFolderName))
            {
                var rdDirectoryPath = installDirPath.Substring(0, installDirPath.IndexOf(rdFolderName) + rdFolderName.Length);
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

            return String.Empty;
        }

        protected abstract String GetWargameName();

        protected abstract String GetWargameFolderName();

        protected abstract IEnumerable<String> GetPotentialInstallDirectoryPaths();

        protected abstract IEnumerable<String> GetPotentialInstallRegistryKeys();

    }
}
