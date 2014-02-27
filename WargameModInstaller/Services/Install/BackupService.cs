using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstaller.Services.Install
{
    /// <summary>
    /// 
    /// </summary>
    public class BackupService : IBackupService, IProgressProvider
    {
        private readonly String backupStoreName;

        public BackupService()
        {
            this.backupStoreName = WargameModInstaller.Properties.Resources.AppName;
            this.Backups = new Dictionary<String, String>();
            this.BackupDirectories = new HashSet<String>();
        }

        /// <summary>
        /// The dictionary of backups.
        /// The orginal path to a backup file is a key, backup path is a value.
        /// </summary>
        protected Dictionary<String, String> Backups
        {
            get;
            set;
        }

        /// <summary>
        /// Set of the directories used for backups.
        /// </summary>
        protected HashSet<String> BackupDirectories
        {
            get;
            set;
        }

        /// <summary>
        /// Backups a file wtih a given path.
        /// </summary>
        /// <param name="file"></param>
        public void Backup(String file)
        {
            BackupInternal(new[] { file });
        }

        /// <summary>
        /// Backups a file wtih a given path.
        /// </summary>
        /// <param name="file"></param>>
        /// <param name="token"></param>
        public void Backup(String file, CancellationToken token)
        {
            BackupInternal(new[] { file }, token);
        }

        /// <summary>
        /// Backups all files in the given collection of file paths.
        /// </summary>
        /// <param name="files"></param>
        public void Backup(IEnumerable<String> files)
        {
            BackupInternal(files);
        }

        /// <summary>
        /// Backups all files in the given collection of file paths.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="token"></param>
        public void Backup(IEnumerable<String> files, CancellationToken token)
        {
            BackupInternal(files, token);
        }

        /// <summary>
        /// Backups a file wtih a given path which is relative to the given source directory.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="sourceDir"></param>
        public void BackupRelative(String file, String sourceDir)
        {
            BackupInternal(new[] { Path.Combine(sourceDir, file) });
        }

        /// <summary>
        /// Backups a file wtih a given path which is relative to the given source directory.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="sourceDir"></param>
        /// <param name="token"></param>
        public void BackupRelative(String file, String sourceDir, CancellationToken token)
        {
            BackupInternal(new[] { Path.Combine(sourceDir, file) }, token);
        }

        /// <summary>
        /// Backups all files in the given collection of file paths which are relative to the given source directory.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="sourceDir"></param>
        public void BackupRelative(IEnumerable<String> files, String sourceDir)
        {
            var fullPathsList = new List<String>();
            foreach(var file in files)
            {             
                fullPathsList.Add(Path.Combine(sourceDir, file));
            }

            BackupInternal(fullPathsList);
        }

        /// <summary>
        /// Backups all files in the given collection of file paths which are relative to the given source directory.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="sourceDir"></param>
        /// <param name="token"></param>
        public void BackupRelative(IEnumerable<String> files, String sourceDir, CancellationToken token)
        {
            var fullPathsList = new List<String>();
            foreach (var file in files)
            {
                fullPathsList.Add(Path.Combine(sourceDir, file));
            }

            BackupInternal(fullPathsList, token);
        }

        /// <summary>
        /// Restores a file witha a given path.
        /// </summary>
        /// <param name="files"></param>
        public void Restore(String file)
        {
            RestoreInternal(new[] { file });
        }

        /// <summary>
        /// Restores a file with a given path.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="token"></param>
        public void Restore(String file, CancellationToken token)
        {
            RestoreInternal(new[] { file }, token);
        }

        /// <summary>
        /// Restores files in the given collection of file paths.
        /// </summary>
        /// <param name="files"></param>
        public void Restore(IEnumerable<String> files)
        {
            RestoreInternal(files);
        }

        /// <summary>
        /// Restores files in the given collection of file paths.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="token"></param>
        public void Restore(IEnumerable<String> files, CancellationToken token)
        {
            RestoreInternal(files, token);
        }

        /// <summary>
        /// Restores a file with a given file paths which are relative to the given source directory.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="targetDir"></param>
        public void RestoreRelative(String file, String targetDir)
        {
            RestoreInternal(new[] { Path.Combine(targetDir, file) });
        }

        /// <summary>
        /// Restores a file with a given file paths which are relative to the given source directory.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="targetDir"></param>
        /// <param name="token"></param>
        public void RestoreRelative(String file, String targetDir, CancellationToken token)
        {
            RestoreInternal(new[] { Path.Combine(targetDir, file) }, token);
        }

        /// <summary>
        /// Restores all files in the given collection of file paths which are relative to the given source directory.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="targetDir"></param>
        public void RestoreRelative(IEnumerable<String> files, String targetDir)
        {
            var fullPathsList = new List<String>();
            foreach (var file in files)
            {
                fullPathsList.Add(Path.Combine(targetDir, file));
            }

            RestoreInternal(fullPathsList);
        }

        /// <summary>
        /// Restores all files in the given collection of file paths which are relative to the given source directory.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="targetDir"></param>
        /// <param name="token"></param>
        public void RestoreRelative(IEnumerable<String> files, String targetDir, CancellationToken token)
        {
            var fullPathsList = new List<String>();
            foreach (var file in files)
            {
                fullPathsList.Add(Path.Combine(targetDir, file));
            }

            RestoreInternal(fullPathsList, token);
        }

        /// <summary>
        /// Restores all files, backuped by the service, to their orginal locations.
        /// </summary>
        public void RestoreAll()
        {
            TotalSteps = Backups.Count;
            CurrentStep = 0;

            foreach (var backup in Backups)
            {
                var fileOriginPath = backup.Key;
                var fileBackupPath = backup.Value;

                FileUtilities.CopyFileEx(fileBackupPath, fileOriginPath);

                CurrentStep++;
            }

            CurrentStep = TotalSteps;
        }

        /// <summary>
        /// Restores all files, backuped by the service, to their orginal locations.
        /// </summary>
        /// <param name="token"></param>
        public void RestoreAll(CancellationToken token)
        {
            TotalSteps = Backups.Count;
            CurrentStep = 0;

            foreach (var backup in Backups)
            {
                var fileOriginPath = backup.Key;
                var fileBackupPath = backup.Value;

                FileUtilities.CopyFileEx(fileBackupPath, fileOriginPath, token);

                CurrentStep++;
            }

            CurrentStep = TotalSteps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="token"></param>
        protected void BackupInternal(IEnumerable<String> files, CancellationToken? token = null)
        {
            TotalSteps = files.Count();
            CurrentStep = 0;

            foreach (var fileFullPath in files)
            {
                if (!Backups.ContainsKey(fileFullPath))
                {
                    var backupDirPath = TryGetBackupDirForFile(fileFullPath);
                    if (backupDirPath == null)
                    {
                        backupDirPath = CreateBackupDirForFile(fileFullPath);
                        BackupDirectories.Add(backupDirPath);
                    }
                    var fileBackupPath = Path.Combine(backupDirPath, Path.GetRandomFileName());

                    if (token.HasValue)
                    {
                        FileUtilities.CopyFileEx(fileFullPath, fileBackupPath, token.Value);
                    }
                    else
                    {
                        FileUtilities.CopyFileEx(fileFullPath, fileBackupPath);
                    }

                    // Zastnowić się nad tym czy kolejność danych w słowniku jest dobra (w sensie klucz\wartość)
                    Backups.Add(fileFullPath, fileBackupPath);
                }

                CurrentStep++;
            }

            CurrentStep = TotalSteps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="files"></param>
        /// <param name="token"></param>
        protected void RestoreInternal(IEnumerable<String> files, CancellationToken? token = null)
        {
            TotalSteps = files.Count();
            CurrentStep = 0;

            foreach (var fileFullPath in files)
            {
                if (Backups.ContainsKey(fileFullPath))
                {
                    var fileBackupPath = Backups[fileFullPath];

                    if (token.HasValue)
                    {
                        FileUtilities.CopyFileEx(fileBackupPath, fileFullPath, token.Value);
                    }
                    else
                    {
                        FileUtilities.CopyFileEx(fileBackupPath, fileFullPath);
                    }

                    CurrentStep++;
                }
            }

            CurrentStep = TotalSteps;
        }

        /// <summary>
        /// Removes all backuped files managed by the service.
        /// </summary>
        public void Clear()
        {
            TotalSteps = BackupDirectories.Count;
            CurrentStep = 0;

            foreach (var dir in BackupDirectories)
            {
                Directory.Delete(dir, true);

                CurrentStep++;
            }

            Backups.Clear();
            BackupDirectories.Clear();

            CurrentStep = TotalSteps;
        }

        /// <summary>
        /// Computes a total size in bytes of the file with a given path.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected long GetSizeInBytes(String file)
        {
            return GetSizeInBytes(new String[] { file });
        }

        /// <summary>
        /// Computes total size in bytes of the given files.
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        protected long GetSizeInBytes(IEnumerable<String> files)
        {
            long totalSize = 0;
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.Exists)
                {
                    totalSize += fileInfo.Length;
                }
            }

            return totalSize;
        }

        /// <summary>
        /// Tries to get an available backup directory from the known ones for the given file.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected String TryGetBackupDirForFile(String file)
        {
            long fileSize = GetSizeInBytes(file);
            foreach (var backupDir in BackupDirectories)
            {
                var backupDrive = new DriveInfo(backupDir);
                if (backupDrive.AvailableFreeSpace > fileSize)
                {
                    return backupDir;
                }
            }

            return null;
        }

        /// <summary>
        /// Creates a backup directory for the given file basing on the free space required to backup the given file.
        /// </summary>
        /// <param name="file"></param>
        /// <exception cref="System.IO.IOException"></exception>
        /// <returns></returns>
        protected String CreateBackupDirForFile(String file)
        {
            long fileSize = GetSizeInBytes(file);

            var tempDirPath = Path.GetTempPath();
            var tempDirDrive = new DriveInfo(tempDirPath);
            if (tempDirDrive.AvailableFreeSpace > fileSize)
            {
                var backupDirPath = Path.Combine(tempDirPath, backupStoreName);

                PathUtilities.CreateDirectoryIfNotExist(backupDirPath);
                //Create path if not exist
                return backupDirPath;
            }
            else
            {
                var fixedDrives = DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.Fixed);
                foreach(var drive in fixedDrives)
                {
                    if (drive.AvailableFreeSpace > fileSize)
                    {
                        var backupDirPath = Path.Combine(drive.Name, backupStoreName);
                        //Create path if not exist
                        PathUtilities.CreateDirectoryIfNotExist(backupDirPath);
                        var backupDir = new DirectoryInfo(backupDirPath);
                        //backupDir.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                        //hidden //<---------------------------------------------------------------------------------------

                        return backupDirPath;
                    }
                }
            }

            throw new IOException("Not enough disk space for the backup");

        }

        #region IProgressProvider

        private int totalSteps;
        private int currentStep;
        private String currentMessage;

        public event EventHandler<StepChangedEventArgs> CurrentStepChanged = delegate { };
        public event EventHandler<StepChangedEventArgs> TotalStepsChanged = delegate { };
        public event EventHandler<MessageChangedEventArgs> CurrentMessageChanged = delegate { };

        public int TotalSteps
        {
            get
            {
                return totalSteps;
            }
            set
            {
                var oldValue = totalSteps;
                if (oldValue != value)
                {
                    totalSteps = value;
                    OnTotalStepsChanged(totalSteps, oldValue);
                }
            }
        }

        public int CurrentStep
        {
            get
            {
                return currentStep;
            }
            set
            {
                var oldValue = currentStep;
                if (oldValue != value)
                {
                    currentStep = value;
                    OnCurrentStepChanged(currentStep, oldValue);
                }
            }
        }

        public String CurrentMessage
        {
            get
            {
                return currentMessage;
            }
            set
            {
                var oldValue = currentMessage;
                if (currentMessage != value)
                {
                    currentMessage = value;
                    OnCurrentMessageChanged(currentMessage, oldValue);
                }
            }
        }

        /// <summary>
        /// Gets or set the info whether Progress value are kept between operations
        /// or are they computed from the begining each time.
        /// </summary>
        /// <remarks>
        /// TO DO: Impelement
        /// </remarks>
        public bool IsProgressAccumulative
        {
            get;
            set;
        }

        protected void OnTotalStepsChanged(int newValue, int oldValue)
        {
            TotalStepsChanged(this, new StepChangedEventArgs(newValue, oldValue));
        }

        protected void OnCurrentStepChanged(int newValue, int oldValue)
        {
            CurrentStepChanged(this, new StepChangedEventArgs(newValue, oldValue));
        }

        protected void OnCurrentMessageChanged(String newValue, String oldValue)
        {
            CurrentMessageChanged(this, new MessageChangedEventArgs(newValue, oldValue));
        }

        #endregion //IProgressProvider


    }

}
