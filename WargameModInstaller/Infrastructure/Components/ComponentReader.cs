using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using WargameModInstaller.Model;
using WargameModInstaller.Model.Components;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.Infrastructure.Components
{
    /// <summary>
    /// 
    /// </summary>
    public class ComponentReader : IComponentsReader
    {
        private readonly String rootComponentsPath = "WargameModInstallerConfig/InstallCommands/Component";

        public virtual IEnumerable<Component> ReadAll(String filePath)
        {
            var components = new List<Component>();

            try
            {
                XDocument configFile = XDocument.Load(filePath);
                IEnumerable<XElement> rootComponentsElements = configFile.XPathSelectElements(rootComponentsPath);
                foreach (var rootElement in rootComponentsElements)
                {
                    Queue<XElement> elementsQueue = new Queue<XElement>();
                    elementsQueue.Enqueue(rootElement);

                    var rootComponent = ReadComponent(rootElement);

                    Queue<Component> componentsQueue = new Queue<Component>();
                    componentsQueue.Enqueue(rootComponent);

                    while (elementsQueue.Count > 0)
                    {
                        var currentElement = elementsQueue.Dequeue();
                        var currentComponent = componentsQueue.Dequeue();

                        var childrenElements = currentElement.XPathSelectElements("Component");
                        foreach (var childElement in childrenElements)
                        {
                            elementsQueue.Enqueue(childElement);

                            var childComponent = ReadComponent(childElement);
                            currentComponent.AddChild(childComponent);
                            componentsQueue.Enqueue(childComponent);
                        }
                    }

                    components.Add(rootComponent);
                }
            }
            catch (XmlException ex)
            {
                //ok wyglada na to ze nic sie chyba innego stać niż XmlException nie może...
                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);

                throw;
            }

            return components;
        }

        protected virtual Component ReadComponent(XElement source)
        {            
            var name = source.Attribute("name").ValueNullSafe(); //Co z unikalnością imienia
            var text = source.Attribute("text").ValueNullSafe();
            var type = source.Attribute("type").ValueNullSafe();
            bool isMarkedForInstall = source.Attribute("isMarkedForInstall").ValueOr<bool>(true);

            ComponentType componentType = ComponentType.GetDefault();
            if (ComponentType.IsKnownVersion(type))
            {
                componentType = ComponentType.GetByName(type);
            }

            var newComponent = new Component(componentType, name, text);
            newComponent.IsMarkedForInstall = isMarkedForInstall;

            return newComponent;
        }

    }
}
