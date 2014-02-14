using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Services.Config
{
    public class ConfigFileLocator : IConfigFileLocator
    {
        public bool ConfigFileExists()
        {
            var workingDir = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var configFiles = workingDir.GetFiles("*.wmi");

            return configFiles.Length > 0;
        }

        /// <summary>
        /// Gets the installer configuration file path.
        /// </summary>
        /// <returns>Installer config file path or null if not found.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Throws an exception if unable to find any configuration file.</exception>
        public String GetConfigFilePath()
        {
            var workingDir = new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var configFiles = workingDir.GetFiles("*.wmi");
            if (configFiles.Length > 0)
            {
                return configFiles.First().FullName;
            }
            else
            {
                throw new FileNotFoundException("Cannot find any installer configuration file");
            }

        }

    }

}
