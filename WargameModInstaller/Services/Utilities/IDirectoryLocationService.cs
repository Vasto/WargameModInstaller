using System;

namespace WargameModInstaller.Services.Utilities
{
    public interface IDirectoryLocationService
    {
        String SelectedDirectoryPath { get; }
        bool DetermineDirectoryLocation();

    }
}
