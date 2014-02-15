using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.Services.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class ReplaceImageCmdExecutor : ReplaceImageCmdExecutorBase<ReplaceImageCmd>
    {
        public ReplaceImageCmdExecutor(ReplaceImageCmd command)
            : base(command)
        {
            this.TotalSteps = 2;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null)
        {
            CurrentStep = 0;
            CurrentMessage = Command.GetExecutionMessage();

            try
            {
                //Need to add here a command properties validation

                //Cancel if requested;
                token.ThrowIfCanceledAndNotNull();


                String sourceFullPath = Command.SourcePath.GetAbsoluteOrPrependIfRelative(context.InstallerSourceDirectory);
                String targetfullPath = Command.TargetPath.GetAbsoluteOrPrependIfRelative(context.InstallerTargetDirectory);

                //if (!File.Exists(Command.SourceFullPath) ||
                //    !File.Exists(Command.TargetFullPath))
                //{

                //}

                var edataReader = new EdataReader();
                var edataFile = CanGetEdataFromContext(context) ? 
                    GetEdataFromContext(context) :
                    edataReader.ReadAll(targetfullPath, false); //Wprowadzić to wszędzie, najlepiej w formi metod klasy bazowej

                var edataContentFile = GetEdataContentFile(edataFile, Command.TargetContentPath);
                if (!edataContentFile.IsContentLoaded)
                {
                    LoadEdataContentFile(edataReader, edataContentFile);
                }

                TgvImage oldTgv = GetTgvFromEdataContent(edataContentFile);

                TgvImage newtgv = GetTgvFromDDS(sourceFullPath);
                newtgv.SourceChecksum = oldTgv.SourceChecksum;
                newtgv.IsCompressed = oldTgv.IsCompressed;

                byte[] rawNewTgv = ConvertTgvToBytes(newtgv);

                CurrentStep++;

                edataContentFile.Content = rawNewTgv;
                edataContentFile.Size = rawNewTgv.Length;

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
                    //Log only if the command is not a critical one, otherwise, an exception will bubble
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
