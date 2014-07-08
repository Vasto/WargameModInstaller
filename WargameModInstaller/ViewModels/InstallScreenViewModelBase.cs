using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Config;
using WargameModInstaller.Services.Config;
using WargameModInstaller.Services.Utilities;
using WargameModInstaller.ViewModels.Messages;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.ViewModels
{
    /// <summary>
    /// A base class for the installation screen view models.
    /// </summary>
    public abstract class InstallScreenViewModelBase : Screen, IInstallScreen
    {
        private ImageSource backgroundImage;
        private String header;
        private String description;
        private bool canNext;
        private bool canBack;
        private bool canCancel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventAggeregator"></param>
        /// <param name="messageService"></param>
        /// <param name="settingsProvider"></param>
        public InstallScreenViewModelBase(
            IEventAggregator eventAggeregator, 
            IMessageService messageService,
            ISettingsProvider settingsProvider)
        {
            this.EventAggregator = eventAggeregator;
            this.MessageService = messageService;
            this.SettingsProvider = settingsProvider;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventAggeregator"></param>
        /// <param name="messageService"></param>
        /// <param name="settingsProvider"></param>
        /// <param name="header"></param>
        /// <param name="description"></param>
        public InstallScreenViewModelBase(
            IEventAggregator eventAggeregator, 
            IMessageService messageService,
            ISettingsProvider settingsProvider,
            String header, 
            String description)
        {
            this.EventAggregator = eventAggeregator;
            this.MessageService = messageService;
            this.SettingsProvider = settingsProvider;
            this.Header = header;
            this.Description = description;
        }

        public IInstallScreen NextScreen
        {
            get;
            set;
        }

        public IInstallScreen PreviousScreen
        {
            get;
            set;
        }

        public bool CanNext
        {
            get
            {
                return canNext;
            }
            set
            {
                canNext = value;
                NotifyOfPropertyChange(() => CanNext);
            }
        }

        public bool CanBack
        {
            get
            {
                return canBack;
            }
            set
            {
                canBack = value;
                NotifyOfPropertyChange(() => CanBack);
            }
        }

        public bool CanCancel
        {
            get
            {
                return canCancel;
            }
            set
            {
                canCancel = value;
                NotifyOfPropertyChange(() => CanCancel);
            }
        }

        public ImageSource BackgroundImage
        {
            get
            {
                return backgroundImage;
            }
            set
            {
                backgroundImage = value;
                NotifyOfPropertyChange(() => BackgroundImage);
            }
        }

        public String Header
        {
            get
            {
                return header;
            }
            set
            {
                header = value;
                NotifyOfPropertyChange(() => Header);
            }
        }

        public String Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        public String Version
        {
            get
            {
                return MiscUtilities.GetAssemblyVerison();
            }
        }

        protected IEventAggregator EventAggregator
        {
            get;
            set;
        }

        protected IMessageService MessageService
        {
            get;
            set;
        }

        protected ISettingsProvider SettingsProvider
        {
            get;
            set;
        }

        public virtual void Next()
        {
            EventAggregator.Publish(new NextScreenMessage(this));
        }

        public virtual void Back()
        {
            EventAggregator.Publish(new PreviousScreenMessage(this));
        }

        public virtual void Cancel()
        {
            var result = MessageService.Show(
                WargameModInstaller.Properties.Resources.CancelInstallationQuestion,
                WargameModInstaller.Properties.Resources.CancelInstallationHeader,
                MessageButton.YesNo,
                MessageImage.Warning);
            if (result == MessageResult.Yes)
            {
                //OnCanceling();
                EventAggregator.Publish(new InstallCanceledMessage(this));
            }
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            CanCancel = true;
            CanBack = PreviousScreen != null;
            CanNext = NextScreen != null;
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);

            if (SettingsProvider.GetGeneralSettings(GeneralSettingEntryType.AutoInstall)
                .Value
                .ToOrDefault<bool>())
            {
                Next();
            }
        }

    }

}
