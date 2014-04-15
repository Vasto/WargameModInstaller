using System;

namespace WargameModInstaller.ViewModels.Factories
{
    public interface IInstallScreenViewModelFactory
    {
        T Create<T>() where T : IInstallScreen;
        IInstallScreen Create(Type screenType);
        T Create<T>(String header, String description) where T : IInstallScreen;
        IInstallScreen Create(Type screenType, String header, String description);
    }
}
