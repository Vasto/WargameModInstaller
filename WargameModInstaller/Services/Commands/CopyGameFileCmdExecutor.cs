using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Commands;

namespace WargameModInstaller.Services.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class CopyGameFileCmdExecutor : CmdExecutorBase<CopyGameFileCmd>
    {
        public CopyGameFileCmdExecutor(CopyGameFileCmd command)
            : base(command)
        {
            this.TotalSteps = 1;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null)
        {
            CurrentStep = 0;
            CurrentMessage = Command.GetExecutionMessage();


            //Backup cmd operates only in the installer's target dir.
            String sourceFullPath = Path.Combine(context.InstallerTargetDirectory, Command.SourcePath);
            String targetfullPath = Path.Combine(context.InstallerTargetDirectory, Command.TargetPath);
            if (!File.Exists(sourceFullPath) || !File.Exists(targetfullPath))
            {
                throw new CmdExecutionFailedException(
                    "One of the command's Source or Target paths is not a valid file path.",
                    String.Format(WargameModInstaller.Properties.Resources.CopyFileErrorParametrizedMsg, Command.SourcePath));
            }

            PathUtilities.CreateDirectoryIfNotExist(Path.GetDirectoryName(targetfullPath));

            if (token.HasValue)
            {
                FileUtilities.CopyFileEx(sourceFullPath, targetfullPath, token.Value);
            }
            else
            {
                FileUtilities.CopyFileEx(sourceFullPath, targetfullPath);
            }

            CurrentStep = TotalSteps;
        }

    }

}
