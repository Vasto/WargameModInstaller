using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WargameModInstaller.Services.Install.Base
{
    public abstract class WargameProfileLocatorBase : IWargameProfileLocator
    {
        protected abstract String GetWargameSteamAppID();

        public String TryGetProfileRootDirectory()
        {
            String steamInstallLocation = TryGetSteamInstallLocation();
            if (steamInstallLocation != null)
            {
                String profilesRoot = Path.Combine(steamInstallLocation, "userdata");
                return profilesRoot;
            }

            return null;
        }

        /// <summary>
        /// Gets all directories which contain a Wargame: Airland Battle profile files.
        /// </summary>
        /// <returns>A collection of paths to profile directories.</returns>
        public IEnumerable<String> GetProfileDirectories()
        {
            var dirs = new List<String>();

            String steamInstallLocation = TryGetSteamInstallLocation();
            if (steamInstallLocation != null)
            {
                String userataPath = Path.Combine(steamInstallLocation, "userdata");
                var userdataSubDirs = Directory.EnumerateDirectories(userataPath);
                foreach (var dir in userdataSubDirs)
                {
                    var albProfileDir = Path.Combine(dir, GetWargameSteamAppID());
                    if (Directory.Exists(albProfileDir))
                    {
                        dirs.Add(albProfileDir);
                    }
                }
            }

            return dirs;
        }

        private String TryGetSteamInstallLocation()
        {
            var installRegistryKeys = GetPotentialSteamInstallRegistryKeys();
            foreach (var key in installRegistryKeys)
            {
                var installLocation = Registry.GetValue(key, "SteamPath", null) as String;
                if (installLocation != null)
                {
                    if (Directory.Exists(installLocation))
                    {
                        return installLocation;
                    }
                }
            }

            return null;
        }

        private IEnumerable<String> GetPotentialSteamInstallRegistryKeys()
        {
            var results = new List<String>();
            results.Add(@"HKEY_CURRENT_USER\Software\Valve\Steam");

            return results;
        }
    }
}
