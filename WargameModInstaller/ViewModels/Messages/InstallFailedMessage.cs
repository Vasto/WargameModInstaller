using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.ViewModels.Messages
{
    public class InstallFailedMessage : MessageBase
    {
        public InstallFailedMessage(object source, String failMessage)
            : base(source)
        {
            this.FailMessage = failMessage;
        }

        public String FailMessage
        {
            get;
            private set;
        }

    }
}
