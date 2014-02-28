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

namespace WargameModInstaller.Services.Config
{
    public static class WargameVersionProvider
    {
        public static String GetVersion()
        {
            var configFilePath = ConfigFileLocator.GetConfigFilePath();
            try
            {
                XDocument configFile = XDocument.Load(configFilePath);
                XElement element = configFile.XPathSelectElement(
                    "WargameModInstallerConfig/Settings/General/" + GeneralSettingEntryType.WargameVersion);
                if (element != null)
                {
                    return element.Attribute("Version").ValueNullSafe() ?? element.ValueNullSafe();
                }
                else
                {
                    return null;
                }
            }
            catch (XmlException ex)
            {
                WargameModInstaller.Common.Logging.LoggerFactory.Create(typeof(WargameVersionProvider)).Error(ex);

                throw;
            }
        }

    }

}
