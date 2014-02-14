using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Common.Utilities.Image;
using WargameModInstaller.Infrastructure.Config;
using WargameModInstaller.Services.Config;
using WargameModInstaller.Services.Install;
using WargameModInstaller.Services.Utilities;
using WargameModInstaller.ViewModels.Messages;

namespace WargameModInstaller.ViewModels
{
    public class InstallLocationViewModel : InstallScreenViewModelBase
    {
        private readonly IDirectoryLocationService dirLocationService;
        private readonly IWargameInstallDirService installDirProvider;
        private readonly IInstallerService installer;

        private String location;

        public InstallLocationViewModel(
            IEventAggregator eventAggeregator, 
            IMessageService messageService,
            ISettingsProvider settingsProvider,
            IDirectoryLocationService dirLocationService,
            IWargameInstallDirService installDirProvider,
            IInstallerService installer)
            : base(eventAggeregator, messageService, settingsProvider)
        {
            this.dirLocationService = dirLocationService;
            this.installDirProvider = installDirProvider;
            this.installer = installer;

            this.Location = installDirProvider.TryGetInstallDirectory();

            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.LocationScreen);
            this.BackgroundImage = MiscImageUtilities.LoadBitmap(settings.Background);
            this.Header = settings.Header;
            this.Description = settings.Description;
        }

        public InstallLocationViewModel(
            IEventAggregator eventAggeregator,
            IMessageService messageService,
            ISettingsProvider settingsProvider,
            IDirectoryLocationService dirLocationService,
            IWargameInstallDirService installDirProvider,
            IInstallerService installer,
            String header,
            String description)
            : base(eventAggeregator, messageService, settingsProvider, header, description)
        {
            this.dirLocationService = dirLocationService;
            this.installDirProvider = installDirProvider;
            this.installer = installer;

            this.Location = installDirProvider.TryGetInstallDirectory();

            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.LocationScreen);
            this.BackgroundImage = MiscImageUtilities.LoadBitmap(settings.Background);
        }

        public String Location
        {
            get
            {
                return location;
            }
            set
            {
                location = value;
                NotifyOfPropertyChange(() => Location);
            }
        }

        public virtual void SelectLocation()
        {
            var result = dirLocationService.DetermineDirectoryLocation();
            if (result == true)
            {
                Location = dirLocationService.SelectedDirectoryPath;
            }
        }

        public override void Next()
        {
            if (!PathUtilities.IsValidAbsolutePath(Location))
            {
                MessageService.Show(
                    WargameModInstaller.Properties.Resources.InvalidPathDetail,
                    WargameModInstaller.Properties.Resources.InvalidPathHeader,
                    MessageButton.OK,
                    MessageImage.Warning);

                return;
            }

            if (!installDirProvider.IsCorrectInstallDirectory(Location))
            {
                var result = MessageService.Show(
                    WargameModInstaller.Properties.Resources.InvalidWargameDirQuestion,
                    WargameModInstaller.Properties.Resources.InvalidWargameDirHeader,
                    MessageButton.YesNo,
                    MessageImage.Warning);
                if (result == MessageResult.No)
                {
                    return;
                }
            }

            installer.InstallLocation = Location;

            EventAggregator.Publish(new NextScreenMessage(this));

            installer.InstallAsync();
        }

    }

}
