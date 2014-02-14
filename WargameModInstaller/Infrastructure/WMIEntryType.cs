using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Config
{
    /// <summary>
    /// An extensible enum like base class, used to represent entry types in the Wargame Mod Installer config file.
    /// </summary>
    public abstract class WMIEntryType : IEquatable<WMIEntryType>
    {
        protected WMIEntryType(String entryName)
        {
            this.EntryName = entryName;
        }

        /// <summary>
        /// Gets the string matching entry type name used in the Wargame Mod Installer config file.
        /// </summary>
        public String EntryName
        {
            get;
            private set;
        }

        public bool Equals(WMIEntryType other)
        {
            WMIEntryType otherEntry = other as WMIEntryType;
            if (otherEntry != null)
            {
                return (otherEntry.EntryName == this.EntryName);
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            WMIEntryType other = obj as WMIEntryType;
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
            return EntryName.GetHashCode();
        }

        public override string ToString()
        {
            return EntryName;
        }

    }

}
