using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Infrastructure.Config
{
    /// <summary>
    /// Provides base for the setting entry types.
    /// </summary>
    public abstract class SettingEntryType : WMIEntryType
    {
        protected SettingEntryType(String entryName)
            : base(entryName)
        {

        }
    }
}
