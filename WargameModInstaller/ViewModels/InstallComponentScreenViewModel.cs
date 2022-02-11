using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities.Image;
using WargameModInstaller.Infrastructure.Components;
using WargameModInstaller.Model.Components;
using WargameModInstaller.Model.Config;
using WargameModInstaller.Services.Config;
using WargameModInstaller.Services.Install;
using WargameModInstaller.Services.Utilities;
using WargameModInstaller.ViewModels.Messages;

namespace WargameModInstaller.ViewModels
{
    public class InstallComponentScreenViewModel : InstallScreenViewModelBase
    {
        private readonly IInstallerService installer;

        private BindableCollection<ComponentViewModel> components;
        private bool isMockupComponentUsed;

        public InstallComponentScreenViewModel(
            IEventAggregator eventAggregator, 
            IMessageService messageService,
            ISettingsProvider settingsProvider,
            IInstallerService installer)
            : base(eventAggregator, messageService, settingsProvider)
        {
            this.installer = installer;

            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.ComponentSelectionScreen);
            this.BackgroundImage = MiscImageUtilities.LoadBitmap(settings.Background);
            this.Header = settings.Header;
            this.Description = settings.Description;

            var componentsModels = GetComponents();
            if (componentsModels.Count() == 0)
            {
                var component = CreateMockupComponent();
                componentsModels = new[] { component };

                isMockupComponentUsed = true;
            }

            this.Components = CreateComponentsViewModels(componentsModels);
        }

        public InstallComponentScreenViewModel(
            IEventAggregator eventAggregator,
            IMessageService messageService, 
            ISettingsProvider settingsProvider,
            IInstallerService installer,
            String header,
            String description)
            : base(eventAggregator, messageService, settingsProvider, header, description)
        {
            this.installer = installer;

            var settings = settingsProvider.GetScreenSettings(ScreenSettingsEntryType.ComponentSelectionScreen);
            this.BackgroundImage = MiscImageUtilities.LoadBitmap(settings.Background);

            var componentsModels = GetComponents();
            if (componentsModels.Count() == 0)
            {
                var component = CreateMockupComponent();
                componentsModels = new[] { component };

                isMockupComponentUsed = true;
            }

            this.Components = CreateComponentsViewModels(componentsModels);
        }

        public BindableCollection<ComponentViewModel> Components
        {
            get
            {
                return components;
            }
            set
            {
                components = value;
                NotifyOfPropertyChange(() => Components);
            }
        }

        public override void Next()
        {
            if (!isMockupComponentUsed)
            {
                var componentsToInstall = GetComponentsToInstall(Components);
                if (componentsToInstall.Count() > 0)
                {
                    installer.ComponentsToInstall = componentsToInstall;
                }
                else
                {
                    MessageService.Show(
                        WargameModInstaller.Properties.Resources.NoComponentsSelectedDetail,
                        WargameModInstaller.Properties.Resources.NoComponentsSelectedHeader,
                        MessageButton.OK,
                        MessageImage.Warning);

                    return;
                }
            }

            EventAggregator.Publish(new NextScreenMessage(this));

            installer.InstallAsync();
        }

        private IEnumerable<Component> GetComponents()
        {
            var reader = new ComponentReader();
            var configFilePath = ConfigFileLocator.GetConfigFilePath();
            var componentsModels = reader.ReadAll(configFilePath);

            return componentsModels;
        }

        private Component CreateMockupComponent()
        {
            var name = "MockupComponent";
            var type = ComponentType.Required;
            var text = SettingsProvider.GetGeneralSettings(GeneralSettingEntryType.ModName).Value;

            var component = new Component(type, name, text);

            return component;
        }

        private BindableCollection<ComponentViewModel> CreateComponentsViewModels(
            IEnumerable<Component> rootComponents)
        {
            var componentsViewModels = new BindableCollection<ComponentViewModel>();

            foreach (var root in rootComponents)
            {
                var builtComponentVM = BuildComponentViewModelHierarchy(root);
                componentsViewModels.Add(builtComponentVM);
            }

            return componentsViewModels;
        }

        private ComponentViewModel BuildComponentViewModelHierarchy(
            Component rootComponent)
        {
            var rootComponentVM = new ComponentViewModel(EventAggregator, MessageService, rootComponent);

            Queue<Component> componentsQueue = new Queue<Component>();
            componentsQueue.Enqueue(rootComponent);

            Queue<ComponentViewModel> componentsVMQueue = new Queue<ComponentViewModel>();
            componentsVMQueue.Enqueue(rootComponentVM);

            //użyte przemierzanie drzewa wszerz żeby zbudować odpowiadające drzewo View Modeli.
            while (componentsQueue.Count > 0)
            {
                var currentComponent = componentsQueue.Dequeue();
                var currentComponentVM = componentsVMQueue.Dequeue();
                foreach (var child in currentComponent.Children)
                {
                    componentsQueue.Enqueue(child);

                    var newComponentVM = new ComponentViewModel(EventAggregator, MessageService, child);
                    componentsVMQueue.Enqueue(newComponentVM);
                    currentComponentVM.Children.Add(newComponentVM);
                }
            }

            return rootComponentVM;
        }

        private IEnumerable<String> GetComponentsToInstall(IEnumerable<ComponentViewModel> componentsVM)
        {
            var componentsToInstall = new List<String>();

            foreach (var vm in componentsVM)
            {
                Queue<ComponentViewModel> vmQueue = new Queue<ComponentViewModel>();
                vmQueue.Enqueue(vm);

                while (vmQueue.Count > 0)
                {
                    var currentVM = vmQueue.Dequeue();

                    var currentComponent = currentVM.WrappedComponent;
                    if (currentComponent.IsMarkedForInstall)
                    {
                        componentsToInstall.Add(currentComponent.Name);

                        foreach (var childVM in currentVM.Children)
                        {
                            vmQueue.Enqueue(childVM);
                        }
                    }
                }
            }

            return componentsToInstall;
        }

    }
}
