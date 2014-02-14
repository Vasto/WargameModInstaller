using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Common.Utilities
{
    /// <summary>
    /// Przechowuje metody użytkowe związane ze ścieżkami do plików lub folderów.
    /// </summary>
    public static class PathUtilities
    {
        private static readonly char[] InvalidChars = 
            Path.GetInvalidPathChars()
            .Union(Path.GetInvalidFileNameChars())
            .Except(new[] { Path.DirectorySeparatorChar })
            .ToArray();

        /// <summary>
        /// Returns whether the given string represents valid local absolute or relative path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidPath(String path)
        {
            return IsValidRelativePath(path) || IsValidAbsolutePath(path);
        }

        /// <summary>
        /// Returns whether the given string represents valid local absolute path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidAbsolutePath(String path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var drives = DriveInfo.GetDrives().Select(d => d.Name);
            if (!drives.Any(d => path.StartsWith(d)))
            {
                return false;
            }

            //Check the path without the root for invalid chars to avoid taking a ':' in the path root as an invalid char.
            var pathWithoutRoot = path.Remove(0, Path.GetPathRoot(path).Length);
            pathWithoutRoot = pathWithoutRoot.Replace(@"\\", ":"); // to cancel out c:\\\\test.txt
            if (pathWithoutRoot.IndexOfAny(InvalidChars) != -1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the given string represents a valid local relative path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidRelativePath(String path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            path = path.Replace(@"\\", ":");
            if (path.IndexOfAny(InvalidChars) != -1)
            {
                return false;
            }

            if (Path.IsPathRooted(path))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the given string represents an existing file or directory path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Exist(String path)
        {
            if (File.Exists(path))
            {
                return true;
            }
            else if (Directory.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Creates directory for the given path if it doesn't exist.
        /// </summary>
        /// <param name="path"></param>
        public static void CreateDirectoryIfNotExist(String path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// Returns whether the given path is a file path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFile(String path)
        {
            FileAttributes attrib = File.GetAttributes(path);
            return ((attrib & FileAttributes.Directory) != FileAttributes.Directory);
        }

        /// <summary>
        /// Returns whether the given path is a directory path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDirectory(String path)
        {
            FileAttributes attrib = File.GetAttributes(path);
            return ((attrib & FileAttributes.Directory) == FileAttributes.Directory);
        }


    }

}
