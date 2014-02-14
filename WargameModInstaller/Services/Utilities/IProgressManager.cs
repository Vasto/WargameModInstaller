using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Services.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public interface IProgressManager
    {
        event EventHandler<ProgressEventArgs> ProgressChanged;
        String CurrentMessage { get; set; }
        int CurrentProgress { get; set; }
        int MaxProgress { get; }
        void SetProgressMin();
        void SetProgressMax();
        //void StepByOne();
        //void StepBy(int amount);
        void Register(IProgressProvider provider);
        void Unregister(IProgressProvider provider);
        void Clear();
        //void GetCurrentProgressFor(IProgressProvider provider);
        //void GetMaxProgressFor(IProgressProvider provider);
    }

}
