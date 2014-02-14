using Caliburn.Micro;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WargameModInstaller.Common.Logging;
using WargameModInstaller.Infrastructure.Commands;
using WargameModInstaller.Infrastructure.Config;
using WargameModInstaller.Model.Config;
using WargameModInstaller.Services.Commands;
using WargameModInstaller.Services.Config;
using WargameModInstaller.Services.Image;
using WargameModInstaller.Services.Install;
using WargameModInstaller.Services.Utilities;
using WargameModInstaller.ViewModels;
using WargameModInstaller.ViewModels.Factories;

namespace WargameModInstaller
{
    class AppBootstrapper : Bootstrapper<ShellViewModel>
    {
        private IKernel kernel;

        static AppBootstrapper()
        {
#if DEBUG
            //LogManager.GetLog = (type) => LoggerFactory.Create(type);
#endif
        }

        public AppBootstrapper()
        {
#if DEBUG
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-us");
#endif
        }

        protected override void Configure()
        {
            kernel = new StandardKernel();

            kernel.Load(AppDomain.CurrentDomain.GetAssemblies());
            kernel.Bind<IEventAggregator>().To<EventAggregator>().InSingletonScope();
            kernel.Bind<IWindowManager>().To<WindowManager>().InSingletonScope();

            kernel.Bind<IInstallScreenViewModelFactory>().To<InstallScreenViewModelFactory>().InSingletonScope();
            kernel.Bind<ICmdExecutorFactory>().To<CmdExecutorFactory>().InSingletonScope();

            kernel.Bind<IInstallCmdReader>().To<InstallCmdReader>();

            kernel.Bind<IMessageService>().To<MessageService>().InSingletonScope();
            kernel.Bind<IDirectoryLocationService>().To<DirectoryLocationService>().InSingletonScope();
            kernel.Bind<IWargameInstallDirService>().To<ALBInstallDirService>().InSingletonScope();
            kernel.Bind<IImageComposerService>().To<ImageComposerService>().InSingletonScope();
            kernel.Bind<IBackupService>().To<BackupService>().InSingletonScope();
            kernel.Bind<IProgressManager>().To<PercentageProgressManager>().InSingletonScope();
            kernel.Bind<IInstallerService>().To<InstallerService>().InSingletonScope();

            kernel.Bind<IGeneralSettingReader>().To<GeneralSettingReader>();
            kernel.Bind<IScreenSettingsReader>().To<ScreenSettingsReader>();
            kernel.Bind<ISettingsProvider>().To<SettingsProvider>().InSingletonScope();
            kernel.Bind<ISettingsFactory>().To<SettingsFactory>().InSingletonScope();
            kernel.Bind<IConfigFileLocator>().To<ConfigFileLocator>().InSingletonScope();
        }

        protected override object GetInstance(Type service, string key)
        {
            return kernel.Get(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return kernel.GetAll(service);
        }

        protected override void BuildUp(object instance)
        {
            kernel.Inject(instance);
        }

        /// <summary>
        /// Metoda zwracająca kolekcję Assembly zawierających potencjalne obiekty View dla Caliburn.Micro.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Domyślnie klasa bazowa zwraca Assembly, w którym istnieje, wiec jeśli widoki są tylko w głównym Assembly applikacji, 
        /// to nie ma potrzeby poszukiwać innych Assembly.
        /// </remarks>
        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            List<Assembly> assemblies = new List<Assembly>();
            assemblies.Add(Assembly.GetExecutingAssembly());
            foreach (var fileName in Directory.EnumerateFiles(Environment.CurrentDirectory, "WargameModInstaller*.dll"))
            {
                assemblies.Add(Assembly.LoadFrom(fileName));
            }

            return assemblies;
        }

        protected override void OnStartup(object sender, System.Windows.StartupEventArgs e)
        {
            bool shutdownRequired = false;
            if (!ConfigFileExists() || !IsConfigFileWellFormed())
            {
                shutdownRequired = true;
            }

            if (shutdownRequired)
            {
                Application.Shutdown();
            }
            else
            {
                //Executing this when shutdown is ordered, prevents application from closing until it fnishes.
                base.OnStartup(sender, e);
            }
        }

        private bool ConfigFileExists()
        {
            var configFileLocator = kernel.Get<IConfigFileLocator>();
            if (!configFileLocator.ConfigFileExists())
            {
                var msgService = kernel.Get<IMessageService>();
                msgService.Show(
                    WargameModInstaller.Properties.Resources.ConfigFileNotFoundErrorMsg,
                    WargameModInstaller.Properties.Resources.FatalErrorHeader,
                    MessageButton.OK,
                    MessageImage.Error);

                return false;
            }
            else
            {
                return true;
            }
        }

        private bool IsConfigFileWellFormed()
        {
            var configFileLocator = kernel.Get<IConfigFileLocator>();
            var configFilePath = configFileLocator.GetConfigFilePath();
            try
            {
                System.Xml.Linq.XDocument configFile = System.Xml.Linq.XDocument.Load(configFilePath);

                return true;
            }
            catch (System.Xml.XmlException ex)
            {
                var msgService = kernel.Get<IMessageService>();
                msgService.Show(
                    ex.Message,
                    WargameModInstaller.Properties.Resources.ConfigFileErrorHeader,
                    MessageButton.OK,
                    MessageImage.Error);

                LoggerFactory.Create(this.GetType()).Error(ex);

                return false;
            }

        }

    }
}
