using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using WargameModInstaller.Common.Extensions;
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
            result.Add(GeneralSettingEntryType.ModName, (source) => ReadModName(source,GeneralSettingEntryType.ModName));
            result.Add(GeneralSettingEntryType.InstallationBackup, (source) => ReadDisableBackup(source, GeneralSettingEntryType.InstallationBackup));
            result.Add(GeneralSettingEntryType.CriticalCommands, (source) => ReadCriticalCommands(source, GeneralSettingEntryType.CriticalCommands));

            return result;
        }

        private GeneralSetting ReadModName(XElement source, GeneralSettingEntryType entryType)
        {
            GeneralSetting result = null;

            var modNameElemet = source.Element(entryType.EntryName);
            if (modNameElemet != null)
            {
                //First read the attribute, because an empty tag element value returns an empty string not null.
                var value = modNameElemet.Attribute("Name").ValueNullSafe() ??
                    modNameElemet.ValueNullSafe();
                if (value != null)
                {
                    result = new GeneralSetting(entryType, value);
                }
            }

            return result;
        }

        private GeneralSetting ReadDisableBackup(XElement source, GeneralSettingEntryType entryType)
        {
            GeneralSetting result = null;

            var modNameElemet = source.Element(entryType.EntryName);
            if (modNameElemet != null)
            {
                var value = modNameElemet.Attribute("Value").ValueNullSafe() ??
                    modNameElemet.ValueNullSafe();
                if (value != null)
                {
                    result = new GeneralSetting(entryType, value);
                }
            }

            return result;
        }

        private GeneralSetting ReadCriticalCommands(XElement source, GeneralSettingEntryType entryType)
        {
            GeneralSetting result = null;

            var modNameElemet = source.Element(entryType.EntryName);
            if (modNameElemet != null)
            {
                var value = modNameElemet.Attribute("Value").ValueNullSafe() ??
                    modNameElemet.ValueNullSafe();
                if (value != null)
                {
                    result = new GeneralSetting(entryType, value);
                }
            }

            return result;
        }

    }

}
