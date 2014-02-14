using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.ViewModels.Messages
{
    /// <summary>
    /// A base class for messages used to communicate between View Models.
    /// </summary>
    public abstract class MessageBase
    {
        public MessageBase(object source)
        {
            this.Source = source;
        }

        /// <summary>
        /// Gets a message sender.
        /// </summary>
        public object Source
        {
            get;
            private set;
        }
    }
}
