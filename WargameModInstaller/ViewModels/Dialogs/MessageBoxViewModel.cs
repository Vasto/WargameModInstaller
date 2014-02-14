using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.ViewModels.Dialogs
{
    public class MessageBoxViewModel : Screen
    {
        private String caption;
        private String text;
        private String acceptButtonDisplayName;
        private String rejectButtonDisplayName;
        private String cancelButtonDisplayName;
        private ImageSource barIcon;

        public MessageBoxViewModel()
        {

        }

        public String Caption
        {
            get
            {
                return caption;
            }
            set
            {
                caption = value;
                NotifyOfPropertyChange(() => Caption);
            }
        }

        public String Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                NotifyOfPropertyChange(() => Text);
            }
        }

        public String AcceptButtonDisplayName
        {
            get
            {
                return acceptButtonDisplayName;
            }
            set
            {
                acceptButtonDisplayName = value;
                NotifyOfPropertyChange(() => AcceptButtonDisplayName);
            }
        }

        public String RejectButtonDisplayName
        {
            get
            {
                return rejectButtonDisplayName;
            }
            set
            {
                rejectButtonDisplayName = value;
                NotifyOfPropertyChange(() => RejectButtonDisplayName);
            }
        }

        public String CancelButtonDisplayName
        {
            get
            {
                return cancelButtonDisplayName;
            }
            set
            {
                cancelButtonDisplayName = value;
                NotifyOfPropertyChange(() => CancelButtonDisplayName);
            }
        }

        public ImageSource BarIcon
        {
            get
            {
                return barIcon;
            }
            set
            {
                barIcon = value;
                NotifyOfPropertyChange(() => BarIcon);
            }
        }

        public void Accept()
        {
            this.TryClose(true);
        }

        public void Reject()
        {
            this.TryClose(false);
        }

        public void Cancel()
        {
            this.TryClose();
        }

    }
}
