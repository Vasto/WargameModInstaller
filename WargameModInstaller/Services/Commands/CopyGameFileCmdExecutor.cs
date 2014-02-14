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
    /// Change to copy?
    /// </summary>
    public class CopyGameFileCmdExecutor : CmdExecutorBase<CopyGameFileCmd>
    {
        public CopyGameFileCmdExecutor(CopyGameFileCmd command) : base(command)
        {
            this.TotalSteps = 1;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null)
        {
            CurrentStep = 0;
            CurrentMessage = Command.GetExecutionMessage();


            try
            {
                //Backup cmd operates only in the installer's target dir.
                String sourceFullPath = Path.Combine(context.InstallerTargetDirectory, Command.SourcePath);
                String targetfullPath = Path.Combine(context.InstallerTargetDirectory, Command.TargetPath);

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
