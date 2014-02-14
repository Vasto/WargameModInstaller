using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Services.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class CmdExecutionContext
    {
        public CmdExecutionContext(String installerSourceDir, String installerTargetDir)
        {
            this.InstallerSourceDirectory = installerSourceDir;
            this.InstallerTargetDirectory = installerTargetDir;
        }

        /// <summary>
        /// Gets the installer's source workng directory.
        /// </summary>
        public String InstallerSourceDirectory
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the installer's target working directory.
        /// </summary>
        public String InstallerTargetDirectory
        {
            get;
            private set;
        }

    }

}
