using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Config;
using WargameModInstaller.Services.Config;
using WargameModInstaller.Services.Utilities;
using WargameModInstaller.ViewModels.Factories;
using WargameModInstaller.ViewModels.Messages;

namespace WargameModInstaller.ViewModels
{
    /// <summary>
    /// 
    /// </summary>
    public class ShellViewModel
        : Conductor<IInstallScreen>.Collection.OneActive,
        IHandle<NextScreenMessage>,
        IHandle<PreviousScreenMessage>,
        IHandle<InstallCanceledMessage>,
        IHandle<InstallCompletedMessage>,
        IHandle<InstallFailedMessage>,
        IHandle<InstallClosedMessage>
    {
        private readonly IWindowManager windowManager;
        private readonly IEventAggregator eventAggregator;
        private readonly IMessageService messageService;
        private readonly ISettingsProvider settingsProvider;
        private readonly IInstallScreenViewModelFactory installScreenFactory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="windowManager"></param>
        /// <param name="eventAggeregator"></param>
        public ShellViewModel(
            IWindowManager windowManager,
            IEventAggregator eventAggregator,
            IMessageService messageService,
            ISettingsProvider settingsProvider,
            IInstallScreenViewModelFactory installScreenFactory)
        {
            this.windowManager = windowManager;
            this.eventAggregator = eventAggregator;
            this.eventAggregator.Subscribe(this);
            this.messageService = messageService;
            this.settingsProvider = settingsProvider;
            this.installScreenFactory = installScreenFactory;

            this.DisplayName = WargameModInstaller.Properties.Resources.AppName;

            var welcomeScreen = installScreenFactory.Create<InstallWelcomeViewModel>();
            this.Items.Add(welcomeScreen);
            this.ActivateItem(welcomeScreen);

            var locationScreen = installScreenFactory.Create<InstallLocationViewModel>();
            this.Items.Add(locationScreen);

            var progressScreen = installScreenFactory.Create<InstallProgressViewModel>();
            this.Items.Add(progressScreen);

            welcomeScreen.NextScreen = locationScreen;
            locationScreen.PreviousScreen = welcomeScreen;
            locationScreen.NextScreen = progressScreen;
            progressScreen.PreviousScreen = locationScreen;
        }

        public void Handle(NextScreenMessage message)
        {
            var sender = message.Source as InstallScreenViewModelBase;
            if (sender != null)
            {
                if (sender.NextScreen != null)
                {
                    this.ActivateItem(sender.NextScreen);
                }
            }
        }

        public void Handle(PreviousScreenMessage message)
        {
            var sender = message.Source as InstallScreenViewModelBase;
            if (sender != null)
            {
                if (sender.PreviousScreen != null)
                {
                    this.ActivateItem(sender.PreviousScreen);
                }
            }
        }

        public void Handle(InstallCanceledMessage message)
        {
            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.InstallCanceledScreen);
            var header = settings.Header;
            var description = settings.Description;

            var canceledScreen = installScreenFactory.Create<InstallFinishViewModel>(header, description);
            canceledScreen.PreviousScreen = Items.Last();
            canceledScreen.NextScreen = null;

            this.ActivateItem(canceledScreen);
        }

        public void Handle(InstallCompletedMessage message)
        {
            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.InstallCompletedScreen);
            var header = settings.Header;
            var description = settings.Description;

            var completedScreen = installScreenFactory.Create<InstallFinishViewModel>(header, description);
            completedScreen.PreviousScreen = Items.Last();
            completedScreen.NextScreen = null;

            this.ActivateItem(completedScreen);
        }

        public void Handle(InstallFailedMessage message)
        {
            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.InstallFailedScreen);
            var header = settings.Header;
            var description = settings.Description;

            var failedScreen = installScreenFactory.Create<InstallFinishViewModel>(header, description);
            failedScreen.PreviousScreen = Items.Last();
            failedScreen.NextScreen = null;

            this.ActivateItem(failedScreen);
        }

        public void Handle(InstallClosedMessage message)
        {
            this.TryClose();
        }


    }
}
