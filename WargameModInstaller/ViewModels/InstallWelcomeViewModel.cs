using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities.Image;
using WargameModInstaller.Infrastructure.Config;
using WargameModInstaller.Services.Config;
using WargameModInstaller.Services.Utilities;

namespace WargameModInstaller.ViewModels
{
    public class InstallWelcomeViewModel : InstallScreenViewModelBase
    {
        public InstallWelcomeViewModel(
            IEventAggregator eventAggeregator, 
            IMessageService messageService,
            ISettingsProvider settingsProvider) 
            : base(eventAggeregator, messageService, settingsProvider)
        {
            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.WelcomeScreen);
            this.BackgroundImage = MiscImageUtilities.LoadBitmap(settings.Background);
            this.Header = settings.Header;
            this.Description = settings.Description;
        }

        public InstallWelcomeViewModel(
            IEventAggregator eventAggeregator,
            IMessageService messageService, 
            ISettingsProvider settingsProvider,
            String header,
            String description)
            : base(eventAggeregator, messageService, settingsProvider, header, description)
        {
            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.WelcomeScreen);
            this.BackgroundImage = MiscImageUtilities.LoadBitmap(settings.Background);
        }

    }
}
