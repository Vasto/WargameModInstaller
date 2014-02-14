using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Services.Config
{
    public interface IConfigFileLocator
    {
        bool ConfigFileExists();
        String GetConfigFilePath();
    }
}
