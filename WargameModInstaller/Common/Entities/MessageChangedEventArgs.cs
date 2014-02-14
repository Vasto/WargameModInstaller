using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Entities
{
    public class MessageChangedEventArgs
    {
        public MessageChangedEventArgs(String newMessage = "", String oldMessage = "")
        {
            this.NewMessage = newMessage;
            this.OldMessage = oldMessage;
        }

        public String NewMessage
        {
            get;
            private set;
        }

        public String OldMessage
        {
            get;
            private set;
        }

    }
}
