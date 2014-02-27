using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Services.Image;

namespace WargameModInstaller.Services.Commands
{
    public class ReplaceImageTileCmdExecutor : ReplaceImageCmdExecutorBase<ReplaceImageTileCmd>
    {
        public ReplaceImageTileCmdExecutor(IImageComposerService imageComposer, ReplaceImageTileCmd command)
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
                    String.Format(WargameModInstaller.Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
            }

            String rootContentPath = Command.TargetContentPath.Split().FirstOrDefault();
            if (rootContentPath == null)
            {
                throw new CmdExecutionFailedException(
                    "Invalid command's TargetContentPath value.",
                    String.Format(WargameModInstaller.Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
            }


            var edataReader = new EdataFileReader();
            var mainEdataFile = CanGetEdataFromContext(context) ?
                GetEdataFromContext(context) :
                edataReader.Read(targetfullPath, false); //Wprowadzić to wszędzie, najlepiej w formie metod klasy bazowej

            //First one is directly from edata file, so needs to be loaded explicitly.
            EdataContentFile rootContentFile = GetEdataContentFileByPath(mainEdataFile, rootContentPath);
            if (!rootContentFile.IsContentLoaded)
            {
                edataReader.LoadContent(rootContentFile);
            }


            //Prepare a list of nested content according to the given paths.
            var contentFilesHierarchy = GetContentFilesHierarchy(mainEdataFile, Command.TargetContentPath.Split());


            var imageContentFile = contentFilesHierarchy.Last();
            if (imageContentFile.FileType != EdataContentFileType.Image)
            {
                throw new CmdExecutionFailedException(
                    "Invalid command's TargetContentPath value. It doesn't point to an image content.",
                    String.Format(WargameModInstaller.Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
            }

            CurrentStep++;

            TgvImage oldTgv = GetTgvFromContent(imageContentFile);
            TgvImage newtgv = GetTgvFromDDS(sourceFullPath);

            ImageComposerService.ReplaceImageTile(oldTgv, newtgv, (uint)Command.TileSize.Value, (uint)Command.Column.Value, (uint)Command.Row.Value);

            byte[] rawOldTgv = ConvertTgvToBytes(oldTgv);


            //Assign content changes for all packages, from bottom to top
            AssignContentUpHierarchy(contentFilesHierarchy, rawOldTgv);


            if (!CanGetEdataFromContext(context))
            {
                IEdataFileWriter edataWriter = new EdataFileWriter();
                if (token.HasValue)
                {
                    edataWriter.Write(mainEdataFile, token.Value);
                }
                else
                {
                    edataWriter.Write(mainEdataFile);
                }
            }

            CurrentStep = TotalSteps;
        }

    }

}
