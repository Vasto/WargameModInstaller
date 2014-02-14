using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WargameModInstaller.Services.Install
{
    public interface IBackupService
    {
        void Backup(IEnumerable<String> files);
        void Backup(IEnumerable<String> files, CancellationToken token);
        void Backup(String file);
        void Backup(String file, CancellationToken token);
        void BackupRelative(IEnumerable<String> files, String sourceDir);
        void BackupRelative(IEnumerable<String> files, String sourceDir, CancellationToken token);
        void BackupRelative(String file, String sourceDir);
        void BackupRelative(String file, String sourceDir, CancellationToken token);

        void Restore(IEnumerable<String> files);
        void Restore(IEnumerable<String> files, CancellationToken token);
        void Restore(String file);
        void Restore(String file, CancellationToken token);
        void RestoreRelative(IEnumerable<String> files, String targetDir);
        void RestoreRelative(IEnumerable<String> files, String targetDir, CancellationToken token);
        void RestoreRelative(String file, String sourceDir);
        void RestoreRelative(String file, String sourceDir, CancellationToken token);
        void RestoreAll();
        void RestoreAll(CancellationToken token);

        void Clear();
    }
}
