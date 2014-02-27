using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Model.Commands;

namespace WargameModInstaller.Services.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class RemoveCmdExecutor : CmdExecutorBase<RemoveFileCmd>
    {
        public RemoveCmdExecutor(RemoveFileCmd command)
            : base(command)
        {
            this.TotalSteps = 1;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null)
        {
            CurrentStep = 0;
            CurrentMessage = Command.GetExecutionMessage();

            token.ThrowIfCanceledAndNotNull();

            String sourceFullPath = Path.Combine(context.InstallerTargetDirectory, Command.SourcePath);
            if (File.Exists(sourceFullPath))
            {
                File.Delete(sourceFullPath);
            }

            token.ThrowIfCanceledAndNotNull();

            CurrentStep = TotalSteps;
        }

    }

}
