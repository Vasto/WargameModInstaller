using System;
using WargameModInstaller.Model.Commands;

namespace WargameModInstaller.Services.Commands
{
    public interface ICmdExecutorFactory
    {
        ICmdExecutor CreateForCommandGroup(ICmdGroup commandGroup);
        ICmdExecutor CreateForCommand(IInstallCmd command);
    }
}
