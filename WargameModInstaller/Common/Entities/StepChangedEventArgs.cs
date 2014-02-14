using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Entities
{
    /// <summary>
    /// 
    /// </summary>
    public class StepChangedEventArgs : EventArgs
    {
        public StepChangedEventArgs()
        {

        }

        public StepChangedEventArgs(int newValue, int oldValue)
        {
            this.NewValue = newValue;
            this.OldValue = oldValue;
        }

        public int NewValue
        {
            get;
            set;
        }

        public int OldValue
        {
            get;
            set;
        }

    }
}
