using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Services.Commands
{
    public class SharedEdataCmdExecutionContext : CmdExecutionContext
    {
        public SharedEdataCmdExecutionContext(String installerSourceDir, String installerTargetDir, EdataFile edataFile)
            : base(installerSourceDir, installerTargetDir)
        {
            this.EdataFile = edataFile;
        }

        /// <summary>
        /// Gets the Edata file for the current context;
        /// </summary>
        public EdataFile EdataFile
        {
            get;
            private set;
        }

    }
}
