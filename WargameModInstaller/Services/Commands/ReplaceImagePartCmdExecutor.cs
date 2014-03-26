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
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Services.Image;

namespace WargameModInstaller.Services.Commands
{
    public class ReplaceImagePartCmdExecutor : ReplaceCmdExecutorBase<ReplaceImagePartCmd>
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


            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            String sourceFullPath = Command.SourcePath.GetAbsoluteOrPrependIfRelative(context.InstallerSourceDirectory);
            String targetfullPath = Command.TargetPath.GetAbsoluteOrPrependIfRelative(context.InstallerTargetDirectory);
            if (!File.Exists(sourceFullPath) || !File.Exists(targetfullPath))
            {
                throw new CmdExecutionFailedException(
                    "One of the command's Source or Target paths is not a valid file path.",
                    String.Format(Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
            }

            String contentPath = Command.TargetContentPath.LastPart;
            if (contentPath == null)
            {
                throw new CmdExecutionFailedException(
                    "Invalid command's TargetContentPath value.",
                    String.Format(Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
            }

            var edataFileReader = new EdataFileReader();
            var contentOwningEdata = CanGetEdataFromContext(context) ?
                GetEdataFromContext(context) :
                edataFileReader.Read(targetfullPath, false); //Wprowadzić to wszędzie, najlepiej w formie metod klasy bazowej

            EdataContentFile contentFile = contentOwningEdata.GetContentFileByPath(contentPath);
            if (!contentFile.IsContentLoaded)
            {
                edataFileReader.LoadContent(contentFile);
            }

            if (contentFile.FileType != EdataContentFileType.Image)
            {
                throw new CmdExecutionFailedException(
                    "Invalid command's TargetContentPath value. It doesn't point to an image content.",
                    String.Format(Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
            }

            CurrentStep++;

            TgvImage oldTgv = GetTgvFromContent(contentFile);
            TgvImage newtgv = GetTgvFromDDS(sourceFullPath);

            ImageComposerService.ReplaceImagePart(oldTgv, newtgv, (uint)Command.XPosition.Value, (uint)Command.YPosition.Value);

            byte[] rawOldTgv = ConvertTgvToBytes(oldTgv);
            contentFile.Content = rawOldTgv;


            if (!CanGetEdataFromContext(context))
            {
                IEdataFileWriter edataWriter = new EdataFileWriter();
                if (token.HasValue)
                {
                    edataWriter.Write(contentOwningEdata, token.Value);
                }
                else
                {
                    edataWriter.Write(contentOwningEdata);
                }
            }

            CurrentStep = TotalSteps;

        }

    }

}
