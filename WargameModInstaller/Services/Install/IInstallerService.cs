using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Services.Install
{
    public interface IInstallerService
    {
        event EventHandler<EventArgs> InstallCompleted;
        event EventHandler<EventArgs> InstallCanceled;
        event EventHandler<EventArgs<String>> InstallFailed;

        event EventHandler<EventArgs> RestoreCompleted;
        event EventHandler<EventArgs> RestoreCanceled;
        event EventHandler<EventArgs<String>> RestoreFailed;

        event EventHandler<EventArgs> CleanupCanceled;
        event EventHandler<EventArgs> CleanupCompleted;
        event EventHandler<EventArgs<String>> CleanupFailed;

        String InstallLocation { get; set; }
        IEnumerable<String> ComponentsToInstall { get; set; }

        void InstallAsync();
        void RestoreAsync();
        void CleanupAsync();

        void Cancel();
    }

}
