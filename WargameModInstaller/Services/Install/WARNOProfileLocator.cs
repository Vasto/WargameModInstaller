using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Services.Install.Base;

namespace WargameModInstaller.Services.Install
{
    /// <summary>
    /// A WARNO profile file locator.
    /// </summary>
    public class WARNOProfileLocator : WargameProfileLocatorBase
    {
        protected override string GetWargameSteamAppID()
        {
            return "1611600";
        }

    }
}
