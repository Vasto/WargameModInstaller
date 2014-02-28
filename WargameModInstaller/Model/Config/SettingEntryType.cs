using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model;

namespace WargameModInstaller.Model.Config
{
    /// <summary>
    /// Provides base for the setting entry types.
    /// </summary>
    public abstract class SettingEntryType : WMIEntryType
    {
        protected SettingEntryType(int value, String name)
            : base(value, name)
        {

        }
    }
}
