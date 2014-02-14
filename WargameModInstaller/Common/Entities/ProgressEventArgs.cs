using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Entities
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs()
        {

        }

        public ProgressEventArgs(int value)
        {
            this.Value = value;
        }

        public ProgressEventArgs(int value, String message)
        {
            this.Value = value;
            this.Message = message;
        }

        public int Value
        {
            get;
            set;
        }

        public String Message
        {
            get;
            set;
        }

    }
}
