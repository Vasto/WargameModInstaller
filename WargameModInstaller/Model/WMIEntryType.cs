using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model
{
    //Add a textual name of an element in the config file.

    /// <summary>
    /// An extensible enum like base class, used to represent entry types in the Wargame Mod Installer config file.
    /// </summary>
    public abstract class WMIEntryType : Enumeration
    {
        protected WMIEntryType(int value, String name)
            : base(value, name)
        {

        }

    }

}
