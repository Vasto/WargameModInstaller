using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Infrastructure.Config;

namespace WargameModInstaller.Model.Config
{
    public class ScreenSettings : IEquatable<ScreenSettings>
    {
        public ScreenSettings(ScreenSettingsEntryType associatedEntryType)
        {
            this.Type = associatedEntryType;
        }

        /// <summary>
        /// Gets associated entry type, for which value is being held by the current object inastance.
        /// </summary>
        public ScreenSettingsEntryType Type
        {
            get;
            private set;
        }

        public ResourcePath Background
        {
            get;
            set;
        }

        public String Header
        {
            get;
            set;
        }

        public String Description
        {
            get;
            set;
        }

        public virtual bool Equals(ScreenSettings other)
        {
            ScreenSettings otherSetting = other as ScreenSettings;
            if (otherSetting != null)
            {
                return (otherSetting.Type == this.Type) &&
                    (otherSetting.Background == this.Background) &&
                    (otherSetting.Header == this.Header) &&
                    (otherSetting.Description == this.Description);
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            ScreenSettings other = obj as ScreenSettings;
            if (other != null)
            {
                return Equals(other);
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
                Description.GetHashCode();

        }

    }

}
