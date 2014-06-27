using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers;

namespace WargameModInstaller.Services.Commands
{
    public class SharedContainerCmdExecContext : CmdExecutionContext
    {
        public SharedContainerCmdExecContext(
            String installerSourceDir,
            String installerTargetDir,
            IContainerFile containerFile)
            : base(installerSourceDir, installerTargetDir)
        {
            this.ContainerFile = containerFile;
        }

        /// <summary>
        /// 
        /// </summary>
        public IContainerFile ContainerFile
        {
            get;
            private set;
        }
    }
}
