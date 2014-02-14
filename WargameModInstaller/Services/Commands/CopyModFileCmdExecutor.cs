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
using WargameModInstaller.Utilities;

namespace WargameModInstaller.Services.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class CopyModFileCmdExecutor : CmdExecutorBase<CopyModFileCmd>
    {
        public CopyModFileCmdExecutor(CopyModFileCmd command)
            : base(command)
        {
            this.TotalSteps = 1;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null)
        {
            CurrentStep = 0;
            CurrentMessage = Command.GetExecutionMessage();

            String sourceFullPath = Path.Combine(context.InstallerSourceDirectory, Command.SourcePath);
            String targetfullPath = Path.Combine(context.InstallerTargetDirectory, Command.TargetPath);

            try
            {
                PathUtilities.CreateDirectoryIfNotExist(Path.GetDirectoryName(targetfullPath));

                if (token.HasValue)
                {
                    FileUtilities.CopyFileEx(sourceFullPath, targetfullPath, token.Value);
                }
                else
                {
                    FileUtilities.CopyFileEx(sourceFullPath, targetfullPath);
                }

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
                    //Log only if command is not a critical one, otherwise, an exception will bubble
                    WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
                }
            }
            catch (Exception ex)
            {
                if (Command.IsCritical)
                {
                    throw new CmdExecutionFailedException(ex.Message,
                        String.Format(WargameModInstaller.Properties.Resources.CopyFileErrorParametrizedMsg, Command.SourcePath),
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
