using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Services.Install
{
    /// <summary>
    /// Represents a Wargame profile file locator.
    /// </summary>
    public interface IWargameProfileLocator
    {
        /// <summary>
        /// Gets a directory which is a root directory for profile directories.
        /// </summary>
        /// <returns>A root directory path or null if not found.</returns>
        String TryGetProfileRootDirectory();

        /// <summary>
        /// Gets all directories which contain a Wargame profile files.
        /// </summary>
        /// <returns>A collection of paths to profile directories.</returns>
        IEnumerable<String> GetProfileDirectories();
    }
}
