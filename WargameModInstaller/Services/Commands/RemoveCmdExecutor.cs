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
using WargameModInstaller.Services.Commands.Base;

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

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken token)
        {
            InitializeProgress();

            token.ThrowIfCancellationRequested();

            String sourceFullPath = Path.Combine(context.InstallerTargetDirectory, Command.SourcePath);
            if (File.Exists(sourceFullPath))
            {
                File.Delete(sourceFullPath);
            }

            SetMaxProgress();
        }

    }

}
