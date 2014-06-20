using System;

namespace WargameModInstaller.Model.Commands
{
    public interface IInstallCmd
    {
        int Id { get; set; }
        int Priority { get; set; }
        bool IsCritical { get; set; }
        String Name { get; }
        String ExecutionMessage { get; }
    }

}
