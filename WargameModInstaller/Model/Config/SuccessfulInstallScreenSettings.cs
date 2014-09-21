using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Config
{
    public class SuccessfulInstallScreenSettings : ScreenSettings
    {
        public SuccessfulInstallScreenSettings() 
            : base(ScreenSettingsEntryType.InstallCompletedScreen)
        {

        }

        public ResourcePath Readme
        {
            get;
            set;
        }

        public override bool Equals(ScreenSettings other)
        {
            SuccessfulInstallScreenSettings otherSetting = other as SuccessfulInstallScreenSettings;
            if (otherSetting != null)
            {
                return (otherSetting.Type == this.Type) &&
                    (otherSetting.Background == this.Background) &&
                    (otherSetting.Header == this.Header) &&
                    (otherSetting.Description == this.Description) &&
                    (otherSetting.Readme == this.Readme);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Type.GetHashCode() +
                Background.GetHashCode() +
                Header.GetHashCode() +
                Description.GetHashCode() +
                Readme.GetHashCode();

        }
    }
}
