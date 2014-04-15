using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Infrastructure.Commands;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Services.Commands;
using WargameModInstaller.Services.Config;
using WargameModInstaller.Services.Utilities;

namespace WargameModInstaller.Services.Install
{
    /// <summary>
    /// 
    /// </summary>
    public class InstallerService : IInstallerService, IProgressProvider
    {
        private readonly IInstallCmdReader installCommandsReader;
        private readonly ICmdExecutorFactory cmdExecutorFactory;
        private readonly IBackupService backupService;
        private readonly IProgressManager progressManager;
        private readonly ISettingsProvider settingsProvider;

        private CancellationTokenSource cancellationTokenSource;
        private List<IProgressProvider> registeredProgressProviders;
        private HashSet<String> backupedFiles;
        private bool hasBackupCompleted;
        private bool isBackupEnabled;

        private bool isInstallRunning;
        private bool isRestoreRunning;
        private bool isCleanupRunning;

        /// <summary>
        /// Initializes a new instance of the InstallerService class.
        /// </summary>
        /// <param name="installCommandsReader"></param>
        /// <param name="cmdExecutorFactory"></param>
        /// <param name="backupService"></param>
        /// <param name="progressManager"></param>
        /// <param name="settingsProvider"></param>
        /// <param name="configFileLocator"></param>
        public InstallerService(
            IInstallCmdReader installCommandsReader, 
            ICmdExecutorFactory cmdExecutorFactory,
            IBackupService backupService,
            IProgressManager progressManager,
            ISettingsProvider settingsProvider)
        {
            this.installCommandsReader = installCommandsReader;
            this.cmdExecutorFactory = cmdExecutorFactory;
            this.backupService = backupService;
            this.progressManager = progressManager;
            this.progressManager.Register(this);
            this.settingsProvider = settingsProvider;

            this.registeredProgressProviders = new List<IProgressProvider>();
            this.backupedFiles = new HashSet<String>();
            this.isBackupEnabled = settingsProvider
                .GetGeneralSettings(Model.Config.GeneralSettingEntryType.InstallationBackup)
                .Value
                .ToOr<bool>(true);
        }

        /// <summary>
        /// Occurs when the installation is successfully completed;
        /// </summary>
        public event EventHandler<EventArgs> InstallCompleted = delegate { };

        /// <summary>
        /// Occurs when the installation was canceled.
        /// </summary>
        public event EventHandler<EventArgs> InstallCanceled = delegate { };

        /// <summary>
        /// Occurs when the installation failed.
        /// </summary>
        public event EventHandler<EventArgs<String>> InstallFailed = delegate { };

        /// <summary>
        /// Occurs when the restore operation is successfully completed;
        /// </summary>
        public event EventHandler<EventArgs> RestoreCompleted = delegate { };

        /// <summary>
        /// Occurs when the restore operation was canceled.
        /// </summary>
        public event EventHandler<EventArgs> RestoreCanceled = delegate { };

        /// <summary>
        /// Occurs when the restore operation failed.
        /// </summary>
        public event EventHandler<EventArgs<String>> RestoreFailed = delegate { };

        /// <summary>
        /// Occurs when the cleanup operation is successfully completed;
        /// </summary>
        public event EventHandler<EventArgs> CleanupCompleted = delegate { };

        /// <summary>
        /// Occurs when the cleanup operation was canceled.
        /// </summary>
        public event EventHandler<EventArgs> CleanupCanceled = delegate { };

        /// <summary>
        /// Occurs when the cleanup operation failed.
        /// </summary>
        public event EventHandler<EventArgs<String>> CleanupFailed = delegate { };

        /// <summary>
        /// Gets or sets the installation directory path.
        /// </summary>
        public String InstallLocation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the list of names of compomenent which are going to be installed, 
        /// during the isntalltion run.
        /// </summary>
        public IEnumerable<String> ComponentsToInstall
        {
            get;
            set;
        }

        /// <summary>
        /// Starts the installation process.
        /// </summary>
        public async void InstallAsync()
        {
            if (isInstallRunning || isRestoreRunning || isCleanupRunning)
            {
                return;  //Or throw?
            }

            isInstallRunning = true;
            cancellationTokenSource = new CancellationTokenSource();
            hasBackupCompleted = false;

            UnregisterProgressProviders(registeredProgressProviders);
            progressManager.SetProgressMin();

            OperationResult installResult = OperationResult.None;
            String errorMessage = String.Empty;

            try
            {
                await RunInstallAsync();

                installResult = OperationResult.Completed;
            }
            catch (OperationCanceledException ex)
            {
                installResult = OperationResult.Canceled;

                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
            }
            catch (CmdExecutionFailedException ex)
            {
                installResult = OperationResult.Failed;
                errorMessage = ex.InstallerMessage ?? ex.Message;

                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
            }
            catch (Exception ex)
            {
                installResult = OperationResult.Failed;
                errorMessage = ex.Message;

                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
            }

            isInstallRunning = false;

            switch (installResult)
            {
                case OperationResult.Completed:
                    NotifyInstallCompleted();
                    break;

                case OperationResult.Canceled:
                    NotifyInstallCanceled();
                    break;

                case OperationResult.Failed:
                    NotifyInstallFailed(errorMessage);
                    break;
            }
        }

        /// <summary>
        /// Resores backuped files.
        /// </summary>
        public async void RestoreAsync()
        {
            if (isInstallRunning || isRestoreRunning || isCleanupRunning)
            {
                return;  //Or throw?
            }

            isRestoreRunning = true;
            cancellationTokenSource = new CancellationTokenSource();

            UnregisterProgressProviders(registeredProgressProviders);
            progressManager.SetProgressMin();

            OperationResult restoreResult = OperationResult.None;
            String errorMessage = String.Empty;

            try
            {
                await RunRestoreAsync();

                restoreResult = OperationResult.Completed;
            }
            catch (OperationCanceledException ex)
            {
                restoreResult = OperationResult.Canceled;

                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
            }
            catch (CmdExecutionFailedException ex)
            {
                restoreResult = OperationResult.Failed;
                errorMessage = ex.InstallerMessage ?? ex.Message;

                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
            }
            catch (Exception ex)
            {
                restoreResult = OperationResult.Failed;
                errorMessage = ex.Message;

                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
            }

            isRestoreRunning = false;

            switch (restoreResult)
            {
                case OperationResult.Completed:
                    NotifyRestoreCompleted();
                    break;

                case OperationResult.Canceled:
                    NotifyRestoreCanceled();
                    break;

                case OperationResult.Failed:
                    NotifyRestoreFailed(errorMessage);
                    break;
            }
        }

        /// <summary>
        /// Cleanups after the installation process.
        /// </summary>
        public async void CleanupAsync()
        {
            if (isInstallRunning || isRestoreRunning || isCleanupRunning)
            {
                return;  //Or throw?
            }

            isCleanupRunning = true;
            cancellationTokenSource = new CancellationTokenSource();

            UnregisterProgressProviders(registeredProgressProviders);
            progressManager.SetProgressMin();

            OperationResult cleanupResult = OperationResult.None;
            String errorMessage = String.Empty;

            try
            {
                await RunCleanupAsync();

                cleanupResult = OperationResult.Completed;
            }
            catch (OperationCanceledException ex)
            {
                cleanupResult = OperationResult.Canceled;

                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
            }
            catch (CmdExecutionFailedException ex)
            {
                cleanupResult = OperationResult.Failed;
                errorMessage = ex.InstallerMessage ?? ex.Message;

                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
            }
            catch (Exception ex)
            {
                cleanupResult = OperationResult.Failed;
                errorMessage = ex.Message;

                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
            }

            isCleanupRunning = false;

            switch (cleanupResult)
            {
                case OperationResult.Completed:
                    NotifyCleanupCompleted();
                    break;

                case OperationResult.Canceled:
                    NotifyCleanupCanceled();
                    break;

                case OperationResult.Failed:
                    NotifyCleanupFailed(errorMessage);
                    break;
            }
        }

        //To do: zrobiæ jeszcze metode clearAsync wywo³ywan¹ jeœli restore zawiedzie?
        //W ten sposób w po³aczeniu z usuwaiem danych o poprawenie przywróconych plikach, mo¿liwe by³o by kilkukrotne wywyo³anie restore, bez negatywnych konsekwencji...
        //A mo¿e wogóle wydzieliæ metode Clear, któr¹ musia³by wywo³ywaæ klient...

        /// <summary>
        /// Cancels any currently running asynchronous installer operation.
        /// </summary>
        public void Cancel()
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                cancellationTokenSource.Cancel();
            }
        }

        //Ten async i await przed Task.Run mo¿na wyrzuciæ, ale nie wiem jak to wp³ynie na mozliwosci, ponoæ tak lepiej dla wydajnoœci
        private async Task RunInstallAsync()
        {
            await Task.Run(() =>
            {
                CurrentMessage = WargameModInstaller.Properties.Resources.InstallerInitializing;

                //Czy to powinno byæ w task, czy wy¿ej...
                PathUtilities.CreateDirectoryIfNotExist(InstallLocation);

                var configFilePath = ConfigFileLocator.GetConfigFilePath();

                IEnumerable<ICmdGroup> commandGroups = null;
                if (ComponentsToInstall != null)
                {
                    commandGroups = installCommandsReader.ReadGroups(configFilePath, ComponentsToInstall);
                }
                else
                {
                    commandGroups = installCommandsReader.ReadGroups(configFilePath);
                }
                var commandGroupsExecutors = CreateCommandGroupsExecutors(commandGroups);

                var progressProvidingExecutors = GetProgressProvidingExecutors(commandGroupsExecutors);
                RegisterProgressProviders(progressProvidingExecutors);

                if (isBackupEnabled)
                {
                    //Zapamietac liste dla iteracyjnego restore?
                    var backupTargetsList = commandGroups
                        .SelectMany(group => group.Commands)
                        .OfType<IHasTarget>()
                        .Select(c => c.TargetPath);

                    //The InstallerService takes control over the backup porogress notification.
                    //Tak wogole to total steps powinno byæ obliczone przed rozpoczeciem operacji zwiekszajacych progrss, zeby unikn¹æ skakania paska progressu.
                    TotalSteps = backupTargetsList.Count();
                    CurrentStep = 0;
                    CurrentMessage = WargameModInstaller.Properties.Resources.InstallerBackupingFiles;

                    foreach (var backupTarget in backupTargetsList)
                    {
                        var fileToBackupPath = Path.Combine(InstallLocation, backupTarget);
                        if (File.Exists(fileToBackupPath))
                        {
                            backupService.Backup(fileToBackupPath, cancellationTokenSource.Token);
                            backupedFiles.Add(fileToBackupPath);
                        }

                        CurrentStep++;

                        //Check for the cancellation
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }

                    //From now, if the installation is interrupted, the installer restores backuped files.
                    hasBackupCompleted = true;
                }

                var executionContext = CreateCmdExecutionContext();
                foreach (var executor in commandGroupsExecutors)
                {
                    executor.Execute(executionContext, cancellationTokenSource.Token);

                    //Check for the cancellation
                    cancellationTokenSource.Token.ThrowIfCancellationRequested();
                }

                progressManager.SetProgressMax();

            }, cancellationTokenSource.Token);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task RunRestoreAsync()
        {
            await Task.Run(() =>
            {
                if (isBackupEnabled && hasBackupCompleted)
                {
                    TotalSteps = backupedFiles.Count;
                    CurrentStep = 0;
                    CurrentMessage = WargameModInstaller.Properties.Resources.InstallerRestoringFiles;

                    foreach (var file in backupedFiles.ToArray())
                    {
                        backupService.Restore(file, cancellationTokenSource.Token);
                        backupedFiles.Remove(file);

                        CurrentStep++;

                        //Check for the cancellation
                        cancellationTokenSource.Token.ThrowIfCancellationRequested();
                    }
                }

                progressManager.SetProgressMax();

            }, cancellationTokenSource.Token);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task RunCleanupAsync()
        {
            await Task.Run(() =>
            {
                TotalSteps = 1;
                CurrentStep = 0;
                CurrentMessage = WargameModInstaller.Properties.Resources.InstallerClearingFiles;

                backupService.Clear();

                progressManager.SetProgressMax();

            }, cancellationTokenSource.Token);
        }

        /// <summary>
        /// Creates the current installation execution context for commands.
        /// </summary>
        /// <returns></returns>
        private CmdExecutionContext CreateCmdExecutionContext()
        {
            return new CmdExecutionContext(
                AppDomain.CurrentDomain.BaseDirectory,
                InstallLocation);
        }

        /// <summary>
        /// Creates executors for the provided commands collection;
        /// </summary>
        /// <param name="commands"></param>
        /// <returns></returns>
        private IEnumerable<ICmdExecutor> CreateCommandExecutors(IEnumerable<IInstallCmd> commands)
        {
            var commandsExecutors = new List<ICmdExecutor>();
            foreach (var cmd in commands)
            {
                var newExecutor = cmdExecutorFactory.CreateForCommand(cmd);
                commandsExecutors.Add(newExecutor);
            }

            return commandsExecutors;
        }


        private IEnumerable<ICmdExecutor> CreateCommandGroupsExecutors(IEnumerable<ICmdGroup> commandGroups)
        {
            var commandsExecutors = new List<ICmdExecutor>();
            foreach (var group in commandGroups)
            {
                var newExecutor = cmdExecutorFactory.CreateForCommandGroup(group);
                commandsExecutors.Add(newExecutor);
            }

            return commandsExecutors;
        }

        /// <summary>
        /// Gets all progress providers from the given executors collection;
        /// </summary>
        /// <param name="executors"></param>
        /// <returns></returns>
        private IEnumerable<IProgressProvider> GetProgressProvidingExecutors(IEnumerable<ICmdExecutor> executors)
        {
            var progressPorvidingExecutors = new List<IProgressProvider>();
            foreach (var execuctor in executors)
            {
                var progressProvider = execuctor as IProgressProvider;
                if (progressProvider != null)
                {
                    progressPorvidingExecutors.Add(progressProvider);
                }
            }

            return progressPorvidingExecutors;
        }

        private void RegisterProgressProviders(IEnumerable<IProgressProvider> progressProviders)
        {
            foreach (var provider in progressProviders.ToArray())
            {
                progressManager.Register(provider);
                if (!registeredProgressProviders.Contains(provider))
                {
                    registeredProgressProviders.Add(provider);
                }
            }
        }

        private void UnregisterProgressProviders(IEnumerable<IProgressProvider> progressProviders)
        {
            foreach (var provider in progressProviders.ToArray())
            {
                progressManager.Unregister(provider);
                if (registeredProgressProviders.Contains(provider))
                {
                    registeredProgressProviders.Remove(provider);
                }
            }
        }

        private void NotifyInstallCompleted()
        {
            InstallCompleted(this, new EventArgs());
        }

        private void NotifyInstallCanceled()
        {
            InstallCanceled(this, new EventArgs());
        }

        private void NotifyInstallFailed(String message = "")
        {
            InstallFailed(this, new EventArgs<String>(message));
        }

        private void NotifyRestoreCompleted()
        {
            RestoreCompleted(this, new EventArgs());
        }

        private void NotifyRestoreCanceled()
        {
            RestoreCanceled(this, new EventArgs());
        }

        private void NotifyRestoreFailed(String message = "")
        {
            RestoreFailed(this, new EventArgs<String>(message));
        }

        private void NotifyCleanupCompleted()
        {
            CleanupCompleted(this, new EventArgs());
        }

        private void NotifyCleanupCanceled()
        {
            CleanupCanceled(this, new EventArgs());
        }

        private void NotifyCleanupFailed(String message = "")
        {
            CleanupFailed(this, new EventArgs<String>(message));
        }

        private enum OperationResult
        {
            None,
            Completed,
            Canceled,
            Failed
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
