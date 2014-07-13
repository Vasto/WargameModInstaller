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
        private static readonly char[] InvalidLocalPathChars = 
            Path.GetInvalidPathChars()
            .Union(Path.GetInvalidFileNameChars())
            .Except(new[] { Path.DirectorySeparatorChar })
            .ToArray();

        private static readonly char[] InvalidContentPathChars =
            Path.GetInvalidPathChars()
            .Union(Path.GetInvalidFileNameChars())
            .Except(new[] { '\\', '/' })
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
            //var pathWithoutRoot = path.Remove(0, Path.GetPathRoot(path).Length);
            var pathWithoutRoot = path.Substring(Path.GetPathRoot(path).Length);
            pathWithoutRoot = pathWithoutRoot.Replace(@"\\", ":"); // to cancel out c:\\\\test.txt
            if (pathWithoutRoot.IndexOfAny(InvalidLocalPathChars) != -1)
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
            if (path.IndexOfAny(InvalidLocalPathChars) != -1)
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
        /// Returns whether the given string represents a valid wargame files content path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsValidContentPath(String path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            var pathWithoutRoot = path.Substring(GetContentPathRoot(path).Length);
            pathWithoutRoot = pathWithoutRoot.Replace(@"\\", ":");

            if (pathWithoutRoot.IndexOfAny(InvalidContentPathChars) != -1)
            {
                return false;
            }

            return true;
        }

        public static String GetContentPathRoot(String path)
        {
            if (path == null)
            {
                return null;
            }

            if (path.Length == 0)
            {
                return String.Empty;
            }

            var indexOfColon = path.IndexOf(':');
            //znaczy że zaczyna się od : czyli błędna ścieżla tzn ":xxx" albo brak czyli relatywna
            if (indexOfColon < 1)
            {
                return String.Empty;
            }
            else if (path.IndexOfAny(new char[] { '\\', '/' }, indexOfColon, 2) == -1)
            {
                return String.Empty;
            }
            else
            {
                return path.Substring(0, indexOfColon + 1);
            }
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

        /// <summary>
        /// Gets a temporary path and name for a given file 
        /// </summary>
        /// <returns></returns>
        public static String GetTemporaryPath(String filePath)
        {
            var fileInfo = new FileInfo(filePath);
            var temporaryEdataPath = Path.Combine(
                fileInfo.DirectoryName,
                Path.GetFileNameWithoutExtension(fileInfo.Name) + ".tmp");

            return temporaryEdataPath;
        }


    }

}
