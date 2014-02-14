using Caliburn.Micro;
using System;
using System.Collections.Generic;

namespace WargameModInstaller.ViewModels
{
    public interface IInstallScreen : IScreen
    {
        String Header { get; set; }
        String Description { get; set; } 
        IInstallScreen NextScreen { get; set; }
        IInstallScreen PreviousScreen { get; set; }
        bool CanBack { get; }
        bool CanCancel { get; }
        bool CanNext { get; }
        void Next();
        void Back();
        void Cancel();
    }

}
