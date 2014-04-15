using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.ViewModels.Factories
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Yeah, I know I shouldn't use the IoC container here...
    /// Other solution would be to use the constructor injection for obtaining needed services instances 
    /// then manually create new objects and just pass them needed dependencies.
    /// Or just to use the Ninject.Extensions.Factory which adds 2 new dependenicies though (Castle.Core)...
    /// </remarks>
    public class InstallScreenViewModelFactory : IInstallScreenViewModelFactory
    {
        private readonly IKernel kernel;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kernel"></param>
        public InstallScreenViewModelFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        public IInstallScreen Create(Type screenType)
        {
            return (IInstallScreen)kernel.Get(screenType);
        }

        public T Create<T>() where T : IInstallScreen
        {
            return kernel.Get<T>();
        }

        public IInstallScreen Create(Type screenType, String header, String description)
        {
            //Now since changed to IInstallScreen, there is no guarantee that the header, description params constructor exists,
            //So the following code isn't the most appropriate  one.
            var construcotor = screenType.GetConstructor(new[] { typeof(string), typeof(string) });
            if (construcotor != null)
            {
                var parameters = construcotor.GetParameters();
                if (parameters
                    .Where(p => (p.Name == "header") || (p.Name == "description"))
                    .Count() == 2)
                {
                    return (IInstallScreen)kernel.Get(
                        screenType,
                        new ConstructorArgument("header", header),
                        new ConstructorArgument("description", description));
                }
            }

            var newInstallScreen = Create(screenType);
            newInstallScreen.Header = header;
            newInstallScreen.Description = description;

            return newInstallScreen;
        }

        public T Create<T>(String header, String description) where T : IInstallScreen
        {
            Type type = typeof(T);
            var construcotor = type.GetConstructor(new[] { typeof(string), typeof(string) });
            if (construcotor != null)
            {
                var parameters = construcotor.GetParameters();
                if (parameters
                    .Where(p => (p.Name == "header") || (p.Name == "description"))
                    .Count() == 2)
                {
                    return kernel.Get<T>(
                        new ConstructorArgument("header", header),
                        new ConstructorArgument("description", description));
                }
            }

            var newInstallScreen = kernel.Get<T>(); ;
            newInstallScreen.Header = header;
            newInstallScreen.Description = description;

            return newInstallScreen;
        }

    }
}
