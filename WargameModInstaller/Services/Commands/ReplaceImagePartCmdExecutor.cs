using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Services.Image;

namespace WargameModInstaller.Services.Commands
{
    public class ReplaceImagePartCmdExecutor : ReplaceImageCmdExecutorBase<ReplaceImagePartCmd>
    {
        public ReplaceImagePartCmdExecutor(IImageComposerService imageComposer, ReplaceImagePartCmd command)
            : base(command)
        {
            this.ImageComposerService = imageComposer;
            this.TotalSteps = 2;
        }

        protected IImageComposerService ImageComposerService
        {
            get;
            set;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null)
        {
            CurrentStep = 0;
            CurrentMessage = Command.GetExecutionMessage();

            try
            {
                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();

                String sourceFullPath = Command.SourcePath.GetAbsoluteOrPrependIfRelative(context.InstallerSourceDirectory);
                String targetfullPath = Command.TargetPath.GetAbsoluteOrPrependIfRelative(context.InstallerTargetDirectory);

                //No chyba w taki sposób jak ja to chce wykorzystać można. 
                //Oprócz tego można pomyśleć nad ExecutionFailedExcepttion
                //if (!File.Exists(Command.SourceFullPath) ||
                //    !File.Exists(Command.TargetFullPath))
                //{

                //}

                var edataReader = new EdataReader();
                var edataFile = CanGetEdataFromContext(context) ?
                    GetEdataFromContext(context) :
                    edataReader.ReadAll(targetfullPath, false);

                //var edataContentFile = LoadEdataContentFile(edataReader, edataFile, Command.EdataContentPath);
                var edataContentFile = GetEdataContentFile(edataFile, Command.TargetContentPath);
                if (!edataContentFile.IsContentLoaded)
                {
                    LoadEdataContentFile(edataReader, edataContentFile);
                }

                TgvImage oldTgv = GetTgvFromEdataContent(edataContentFile);

                TgvImage newtgv = GetTgvFromDDS(sourceFullPath);

                ImageComposerService.ReplaceImagePart(oldTgv, newtgv, (uint)Command.XPosition.Value, (uint)Command.YPosition.Value);

                byte[] rawOldTgv = ConvertTgvToBytes(oldTgv);

                CurrentStep++;

                edataContentFile.Content = rawOldTgv;
                edataContentFile.Size = rawOldTgv.Length;

                if (!CanGetEdataFromContext(context))
                {
                    IEdataWriter edataWriter = new EdataWriter();
                    if (token.HasValue)
                    {
                        edataWriter.Write(edataFile, token.Value);
                    }
                    else
                    {
                        edataWriter.Write(edataFile);
                    }
                }

                CurrentStep++;

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
                        String.Format(WargameModInstaller.Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath),
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
