using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Containers;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Services.Commands.Base;
using WargameModInstaller.Services.Image;

namespace WargameModInstaller.Services.Commands
{
    public class ReplaceImagePartCmdExecutor : ModImageBySourceCmdExecutor<ReplaceImagePartCmd>
    {
        public ReplaceImagePartCmdExecutor(IImageComposerService imageComposer, ReplaceImagePartCmd command)
            : base(command)
        {
            this.ImageComposerService = imageComposer;
            this.DefaultExecutionErrorMsg = String.Format(
                Properties.Resources.ReplaceContentErrorParamMsg,
                Command.SourcePath);
        }

        protected IImageComposerService ImageComposerService
        {
            get;
            set;
        }

        protected override void ExecuteCommandsLogic(CmdsExecutionData data)
        {
            var contentFile = data.ContainerFile.GetContentFileByPath(data.ContentPath);

            if (contentFile.FileType != ContentFileType.Image)
            {
                throw new CmdExecutionFailedException(
                    String.Format("Invalid TargetContentPath: \"{0}\". It doesn't target an image content file.", data.ContentPath),
                    DefaultExecutionErrorMsg);
            }

            TgvImage oldTgv = BytesToTgv(contentFile.Content);
            TgvImage newtgv = DDSFileToTgv(data.ModificationSourcePath, !Command.UseMipMaps);

            ImageComposerService.ReplaceImagePart(
                oldTgv, 
                newtgv, 
                (uint)Command.XPosition.Value, 
                (uint)Command.YPosition.Value);

            var newContent = TgvToBytes(oldTgv, !Command.UseMipMaps);
            contentFile.LoadCustomContent(newContent);
        }

    }
}
