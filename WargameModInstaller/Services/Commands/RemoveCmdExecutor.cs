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

            try
            {
                token.ThrowIfCanceledAndNotNull();

                String sourceFullPath = Path.Combine(context.InstallerTargetDirectory, Command.SourcePath);
                if (File.Exists(sourceFullPath))
                {
                    File.Delete(sourceFullPath);
                }

                token.ThrowIfCanceledAndNotNull();

            }
            catch (OperationCanceledException ex)
            {
                throw;
            }
            catch (CmdExecutionFailedException ex)
            {
                if (Command.IsCritical)
                {
                    throw;
                }
                else
                {
                    //Log only if the command is not a critical one, otherwise, an exception will bubble
                    WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
                }
            }
            catch (Exception ex)
            {
                if (Command.IsCritical)
                {
                    throw new CmdExecutionFailedException(ex.Message,
                        String.Format(WargameModInstaller.Properties.Resources.RemoveFileErrorParametrizedMsg, Command.SourcePath),
                        ex);
                }
                else
                {
                    WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
                }
            }


            CurrentStep = TotalSteps;
        }

    }

}
