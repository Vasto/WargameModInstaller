using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Model;
using WargameModInstaller.Model.Config;

namespace WargameModInstaller.Infrastructure.Config
{
    public class GeneralSettingReader : WMIReaderBase<XElement, GeneralSetting>, IGeneralSettingReader
    {
        public GeneralSetting Read(String file, GeneralSettingEntryType entryType)
        {
            GeneralSetting result = null;
            if (ReadingQueries.ContainsKey(entryType))
            {
                try
                {
                    XDocument configFile = XDocument.Load(file);
                    XElement rootElement = configFile.XPathSelectElement("WargameModInstallerConfig/Settings/General");
                    if (rootElement != null)
                    {
                        result = ReadingQueries[entryType](rootElement);
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

        protected override Dictionary<WMIEntryType, Func<XElement, GeneralSetting>> CreateReadingQueries()
        {
            var result = new Dictionary<WMIEntryType, Func<XElement, GeneralSetting>>();

            result.Add(GeneralSettingEntryType.ModName, (source) => 
                ReadSetting(source, GeneralSettingEntryType.ModName, "Name"));

            result.Add(GeneralSettingEntryType.InstallationBackup, (source) => 
                ReadSetting(source, GeneralSettingEntryType.InstallationBackup, "Value"));

            result.Add(GeneralSettingEntryType.CriticalCommands, (source) => 
                ReadSetting(source, GeneralSettingEntryType.CriticalCommands, "Value"));

            result.Add(GeneralSettingEntryType.WargameVersion, (source) => 
                ReadSetting(source, GeneralSettingEntryType.WargameVersion, "Version"));

            result.Add(GeneralSettingEntryType.AutoInstall, (source) =>
                ReadSetting(source, GeneralSettingEntryType.AutoInstall, "Value"));

            return result;
        }

        private GeneralSetting ReadSetting(XElement source, GeneralSettingEntryType entry, String attribute)
        {
            GeneralSetting result = null;

            var element = source.Element(entry.Name);
            if (element != null)
            {
                //First read the attribute, because an empty tag element value returns an empty string not null.
                var value = element.Attribute(attribute).ValueNullSafe() ??
                    element.ValueNullSafe();
                if (value != null)
                {
                    result = new GeneralSetting(entry, value);
                }
            }

            return result;
        }

    }

}
