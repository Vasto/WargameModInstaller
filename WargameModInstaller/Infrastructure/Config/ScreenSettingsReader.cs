using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model;
using WargameModInstaller.Model.Config;
using WargameModInstaller.Properties;

namespace WargameModInstaller.Infrastructure.Config
{
    public class ScreenSettingsReader : WMIReaderBase<XElement, ScreenSettings>, IScreenSettingsReader
    {
        public ScreenSettings Read(String file, ScreenSettingsEntryType entry)
        {
            ScreenSettings result = null;
            if (ReadingQueries.ContainsKey(entry))
            {
                try
                {
                    XDocument configFile = XDocument.Load(file);
                    XElement rootElement = configFile.XPathSelectElement("WargameModInstallerConfig/Settings/UI/Screens");
                    if (rootElement != null)
                    {
                        result = ReadingQueries[entry](rootElement);
                    }
                }
                catch (XmlException ex)
                {
                    WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);

                    throw;
                }
            }

            return result;
        }

        protected override Dictionary<WMIEntryType, Func<XElement, ScreenSettings>> CreateReadingQueries()
        {
            var result = new Dictionary<WMIEntryType, Func<XElement, ScreenSettings>>();
            result.Add(ScreenSettingsEntryType.WelcomeScreen, (source) => ReadScreenSettings(source, ScreenSettingsEntryType.WelcomeScreen));
            result.Add(ScreenSettingsEntryType.LocationScreen, (source) => ReadScreenSettings(source, ScreenSettingsEntryType.LocationScreen));
            result.Add(ScreenSettingsEntryType.ProgressScreen, (source) => ReadScreenSettings(source, ScreenSettingsEntryType.ProgressScreen));
            result.Add(ScreenSettingsEntryType.InstallCompletedScreen, (source) => ReadScreenSettings(source, ScreenSettingsEntryType.InstallCompletedScreen));
            result.Add(ScreenSettingsEntryType.InstallCanceledScreen, (source) => ReadScreenSettings(source, ScreenSettingsEntryType.InstallCanceledScreen));
            result.Add(ScreenSettingsEntryType.InstallFailedScreen, (source) => ReadScreenSettings(source, ScreenSettingsEntryType.InstallFailedScreen));
            result.Add(ScreenSettingsEntryType.ComponentSelectionScreen, (source) => ReadScreenSettings(source, ScreenSettingsEntryType.ComponentSelectionScreen));

            return result;
        }

        private ScreenSettings ReadScreenSettings(XElement source, ScreenSettingsEntryType entryType)
        {
            ScreenSettings result = null;

            var screenElemet = source.Element(entryType.Name);
            if (screenElemet != null)
            {
                result = new ScreenSettings(entryType);

                XElement headerElement = screenElemet.Element("Header");
                if (headerElement != null)
                {
                    result.Header = headerElement.Attribute("Text").ValueNullSafe() ??
                        headerElement.ValueNullSafe();
                }

                XElement descriptionElement = screenElemet.Element("Description");
                if (descriptionElement != null)
                {
                    result.Description = descriptionElement.Attribute("Text").ValueNullSafe() ??
                        descriptionElement.ValueNullSafe();
                }

                XElement imagePathElement = screenElemet.Element("BackgroundImage");
                if (imagePathElement != null)
                {
                    String imagePath = imagePathElement.Attribute("Path").ValueNullSafe() ??
                        imagePathElement.ValueNullSafe();

                    result.Background = PathUtilities.IsValidRelativePath(imagePath) ?
                        new ResourcePath(imagePath) :
                        null;
                }
            }

            return result;
        }

    }

}
