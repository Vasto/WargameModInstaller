using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Services.Commands
{
    public class CmdExecutionFailedException : Exception
    {
        public CmdExecutionFailedException(String exceptionMessage)
            : base(exceptionMessage)
        {

        }

        public CmdExecutionFailedException(String exceptionMessage, Exception innerException)
            : base(exceptionMessage, innerException)
        {

        }

        public CmdExecutionFailedException(String exceptionMessage, String installerMessage) 
            : base(exceptionMessage)
        {
            this.InstallerMessage = installerMessage;
        }

        public CmdExecutionFailedException(String exceptionMessage, String installerMessage, Exception innerException)
            : base(exceptionMessage, innerException)
        {
            this.InstallerMessage = installerMessage;
        }

        public String InstallerMessage
        {
            get;
            private set;
        }
    }
}
