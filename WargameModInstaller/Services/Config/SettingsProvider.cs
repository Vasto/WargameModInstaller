using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Infrastructure.Config;
using WargameModInstaller.Model.Config;

namespace WargameModInstaller.Services.Config
{
    public class SettingsProvider : ISettingsProvider
    {
        private readonly IGeneralSettingReader generalSettingsReader;
        private readonly IScreenSettingsReader screenSettingsReader;
        private readonly ISettingsFactory settingsFactory;

        private String configFilePath;

        private Dictionary<String, Func<String>> placeholderReplacingFuncs;

        public SettingsProvider(
            IGeneralSettingReader generalSettingsReader, 
            IScreenSettingsReader screenSettingsReader,
            ISettingsFactory settingsFactory)
        {
            this.generalSettingsReader = generalSettingsReader;
            this.screenSettingsReader = screenSettingsReader;
            this.settingsFactory = settingsFactory;
            this.configFilePath = ConfigFileLocator.GetConfigFilePath();
            this.placeholderReplacingFuncs = CreateTextPlaceholderReplacingFuncs();
            this.ReplacePlaceholdersInDefaultScreenText = true;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ReplacePlaceholdersInDefaultScreenText
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entryType"></param>
        /// <returns></returns>
        public GeneralSetting GetGeneralSettings(GeneralSettingEntryType entryType)
        {
            GeneralSetting settingsValue = generalSettingsReader.Read(configFilePath, entryType);
            if (settingsValue == null)
            {
                //get a default state setting if the reader is unable to read one from the config file
                settingsValue = settingsFactory.CreateSettings<GeneralSetting>(entryType);
            }

            return settingsValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entryType"></param>
        /// <returns></returns>
        public ScreenSettings GetScreenSettings(ScreenSettingsEntryType entryType)
        {
            ScreenSettings settingsValue = screenSettingsReader.Read(configFilePath, entryType);
            if (settingsValue == null)
            {
                //get a default state settings if the reader is unable to read one from the config file
                settingsValue = settingsFactory.CreateSettings<ScreenSettings>(entryType);
            }
            else
            {
                //If any of the properties doesn't have value, assign a default one
                if (settingsValue.Header == null)
                {
                    settingsValue.Header = settingsFactory.CreateSettings<ScreenSettings>(entryType).Header;
                }

                if (settingsValue.Description == null)
                {
                    settingsValue.Description = settingsFactory.CreateSettings<ScreenSettings>(entryType).Description;
                }

                if (settingsValue.Background == null || 
                    settingsValue.Background.PathType == ResourcePathType.Unknown ||
                    !ResourceExist(settingsValue.Background))
                {
                    settingsValue.Background = settingsFactory.CreateSettings<ScreenSettings>(entryType).Background;
                }
            }

            if (ReplacePlaceholdersInDefaultScreenText)
            {
                ReplaceScreenSettingsTextPlaceholders(settingsValue);
            }

            return settingsValue;
        }

        private bool ResourceExist(ResourcePath path)
        {
            bool result = false;

            if (path.PathType == ResourcePathType.Embedded)
            {
                var resourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
                result = resourceNames.Contains(path);
            }
            else if (path.PathType == ResourcePathType.LocalAbsolute)
            {
                result = File.Exists(path);
            }
            else if(path.PathType == ResourcePathType.LocalRelative)
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
                result = File.Exists(path);
            }

            return result;
        }

        private void ReplaceScreenSettingsTextPlaceholders(ScreenSettings settingsValue)
        {
            var placeholders = placeholderReplacingFuncs.Keys;
            foreach (var placeholder in placeholders)
            {
                var replacement = placeholderReplacingFuncs[placeholder]();

                settingsValue.Header = settingsValue.Header.Replace(placeholder, replacement);
                settingsValue.Description = settingsValue.Description.Replace(placeholder, replacement);
            }
        }

        private Dictionary<String, Func<String>> CreateTextPlaceholderReplacingFuncs()
        {
            var result = new Dictionary<String, Func<String>>();

            result.Add("$ModName", () => GetGeneralSettings(GeneralSettingEntryType.ModName).Value);

            result.Add("$WargameVersion", () => {
                return WargameVersionType.GetByName(GetGeneralSettings(GeneralSettingEntryType.WargameVersion).Value).FullName;
            });

            return result;
        }

    }

}
