using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities.Image;
using WargameModInstaller.Model.Config;
using WargameModInstaller.Services.Config;
using WargameModInstaller.Services.Utilities;
using WargameModInstaller.ViewModels.Messages;

namespace WargameModInstaller.ViewModels
{
    public class InstallFinishViewModel : InstallScreenViewModelBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventAggeregator"></param>
        /// <param name="messageService"></param>
        public InstallFinishViewModel(
            IEventAggregator eventAggeregator,
            IMessageService messageService, 
            ISettingsProvider settingsProvider) 
            : base(eventAggeregator, messageService, settingsProvider)
        {
            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.InstallCompletedScreen);
            this.Header = settings.Header;
            this.Description = settings.Description;
            this.BackgroundImage = MiscImageUtilities.LoadBitmap(settings.Background);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventAggeregator"></param>
        /// <param name="messageService"></param>
        /// <param name="header"></param>
        /// <param name="description"></param>
        public InstallFinishViewModel(
            IEventAggregator eventAggeregator,
            IMessageService messageService, 
            ISettingsProvider settingsProvider,
            String header,
            String description)
            : base(eventAggeregator, messageService, settingsProvider, header, description)
        {
            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.InstallCompletedScreen);
            this.BackgroundImage = MiscImageUtilities.LoadBitmap(settings.Background);
        }

        public virtual void Finish()
        {
            EventAggregator.Publish(new InstallClosedMessage(this));
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            CanCancel = true;
            CanBack = false;
            CanNext = false;
        }


    }
}
