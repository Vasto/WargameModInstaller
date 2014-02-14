using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Config;

namespace WargameModInstaller.Model.Config
{
    public class GeneralSetting : IEquatable<GeneralSetting>
    {
        //public GeneralSetting(String settingValue)
        //{
        //    this.Value = settingValue;
        //}

        public GeneralSetting(GeneralSettingEntryType associatedEntryType, String settingValue)
        {
            this.Type = associatedEntryType;
            this.Value = settingValue;
        }

        /// <summary>
        /// Gets associated entry type, for which value is being held by the current object inastance.
        /// </summary>
        public GeneralSettingEntryType Type
        {
            get; 
            private set;
        }

        public String Value
        {
            get;
            private set;
        }

        public bool Equals(GeneralSetting other)
        {
            GeneralSetting otherSetting = other as GeneralSetting;
            if (otherSetting != null)
            {
                return (otherSetting.Type == this.Type) &&
                    (otherSetting.Value == this.Value);
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            GeneralSetting other = obj as GeneralSetting;
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
                Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

    }

}
