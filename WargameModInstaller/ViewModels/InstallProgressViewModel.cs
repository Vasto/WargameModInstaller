using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Utilities.Image;
using WargameModInstaller.Infrastructure.Config;
using WargameModInstaller.Services.Config;
using WargameModInstaller.Services.Install;
using WargameModInstaller.Services.Utilities;
using WargameModInstaller.ViewModels.Messages;

namespace WargameModInstaller.ViewModels
{
    public class InstallProgressViewModel : InstallScreenViewModelBase
    {
        private readonly IInstallerService installer;
        private readonly IProgressManager progressManager;

        private int progress;
        private String message;
        private MessageBase installEndResultMessage;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventAggeregator"></param>
        /// <param name="messageService"></param>
        /// <param name="progressManager"></param>
        /// <param name="installer"></param>
        public InstallProgressViewModel(
            IEventAggregator eventAggeregator,
            IMessageService messageService, 
            ISettingsProvider settingsProvider,
            IProgressManager progressManager,
            IInstallerService installer)
            : base(eventAggeregator, messageService, settingsProvider)
        {
            //To zamienione miejscem z tym udo³u, spr czy nie ma konsekwencji.
            this.progressManager = progressManager;
            this.progressManager.ProgressChanged += ProgressManagerProgressChangedHandler;

            this.installer = installer;
            this.installer.InstallCompleted += InstallCompletedHandler;
            this.installer.InstallCanceled += InstallCanceledHandler;
            this.installer.InstallFailed += InstallFailedHandler;

            this.installer.RestoreCompleted += RestoreCompletedHandler;
            this.installer.RestoreCanceled += RestoreCanceledHandler;
            this.installer.RestoreFailed += RestoreFailedHandler;

            this.installer.CleanupCompleted += CleanupCompletedHandler;
            this.installer.CleanupCanceled += CleanupCanceledHandler;
            this.installer.CleanupFailed += CleanupFailedHandler;

            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.ProgressScreen);
            this.BackgroundImage = MiscImageUtilities.LoadBitmap(settings.Background);
            this.Header = settings.Header;
            this.Description = settings.Description;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventAggeregator"></param>
        /// <param name="messageService"></param>
        /// <param name="progressManager"></param>
        /// <param name="installer"></param>
        /// <param name="header"></param>
        /// <param name="description"></param>
        public InstallProgressViewModel(
            IEventAggregator eventAggeregator,
            IMessageService messageService,
            ISettingsProvider settingsProvider,
            IProgressManager progressManager,
            IInstallerService installer,
            String header,
            String description)
            : base(eventAggeregator, messageService, settingsProvider, header, description)
        {
            this.installer = installer;
            this.installer.InstallCompleted += InstallCompletedHandler;
            this.installer.InstallCanceled += InstallCanceledHandler;
            this.installer.InstallFailed += InstallFailedHandler;

            this.installer.RestoreCompleted += RestoreCompletedHandler;
            this.installer.RestoreCanceled += RestoreCanceledHandler;
            this.installer.RestoreFailed += RestoreFailedHandler;

            this.installer.CleanupCompleted += CleanupCompletedHandler;
            this.installer.CleanupCanceled += CleanupCanceledHandler;
            this.installer.CleanupFailed += CleanupFailedHandler;

            this.progressManager = progressManager;
            this.progressManager.ProgressChanged += ProgressManagerProgressChangedHandler;

            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.ProgressScreen);
            this.BackgroundImage = MiscImageUtilities.LoadBitmap(settings.Background);
        }

        public int Progress
        {
            get
            {
                return progress;
            }
            set
            {
                progress = value;
                NotifyOfPropertyChange(() => Progress);
            }
        }

        public String Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                NotifyOfPropertyChange(() => Message);
            }
        }

        public override void Cancel()
        {
            var result = MessageService.Show(
                WargameModInstaller.Properties.Resources.CancelInstallationQuestion,
                WargameModInstaller.Properties.Resources.CancelInstallationHeader,
                MessageButton.YesNo,
                MessageImage.Warning);
            if (result == MessageResult.Yes)
            {
                installer.Cancel();

                this.Header = WargameModInstaller.Properties.Resources.ProgressScreenCancelingHeader;
                this.Description = WargameModInstaller.Properties.Resources.ProgressScreenCancelingDetail;
                CanCancel = false;
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            CanCancel = true;
            CanBack = false;
            CanNext = false;
        }

        private void InstallCompletedHandler(object sender, EventArgs e)
        {
            installEndResultMessage = new InstallCompletedMessage(this);

            installer.CleanupAsync();
        }

        private void InstallCanceledHandler(object sender, EventArgs e)
        {
            installEndResultMessage = new InstallCanceledMessage(this);

            installer.RestoreAsync();
        }

        private void InstallFailedHandler(object sender, EventArgs<String> e)
        {
            var result = MessageService.Show(
                e.Value,
                WargameModInstaller.Properties.Resources.FatalErrorHeader,
                MessageButton.OK,
                MessageImage.Error);

            this.Header = WargameModInstaller.Properties.Resources.ProgressScreenTerminatigHeader;
            this.Description = WargameModInstaller.Properties.Resources.ProgressScreenTerminatigDetail;

            installEndResultMessage = new InstallFailedMessage(this, e.Value);

            installer.RestoreAsync();
        }


        private void RestoreCompletedHandler(object sender, EventArgs e)
        {
            installer.CleanupAsync();
        }

        private void RestoreCanceledHandler(object sender, EventArgs e)
        {
            installer.CleanupAsync();
        }

        private void RestoreFailedHandler(object sender, EventArgs<String> e)
        {
            installer.CleanupAsync();
        }


        private void CleanupCompletedHandler(object sender, EventArgs e)
        {
            EventAggregator.Publish(installEndResultMessage);
        }

        private void CleanupCanceledHandler(object sender, EventArgs e)
        {
            EventAggregator.Publish(installEndResultMessage);
        }

        private void CleanupFailedHandler(object sender, EventArgs<string> e)
        {
            EventAggregator.Publish(installEndResultMessage);
        }


        private void ProgressManagerProgressChangedHandler(object sender, ProgressEventArgs e)
        {
            Progress = e.Value;
            Message = e.Message;
        }

    }

}
