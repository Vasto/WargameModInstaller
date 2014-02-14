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
    public interface IProgressProvider
    {
        event EventHandler<StepChangedEventArgs> CurrentStepChanged;
        event EventHandler<StepChangedEventArgs> TotalStepsChanged;
        event EventHandler<MessageChangedEventArgs> CurrentMessageChanged;
        int CurrentStep { get; }
        int TotalSteps { get; }
        String CurrentMessage { get; }
    }

}
