using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Services.Image;

namespace WargameModInstaller.Services.Commands
{
    public class ReplaceImageTileCmdExecutor : ImageTargetCmdExecutorBase<ReplaceImageTileCmd>
    {
        public ReplaceImageTileCmdExecutor(IImageComposerService imageComposer, ReplaceImageTileCmd command)
            : base(command)
        {
            this.ImageComposerService = imageComposer;
        }

        protected IImageComposerService ImageComposerService
        {
            get;
            set;
        }

        protected override byte[] ModifyImageContent(byte[] orginalImageContent, String sourceImagePath)
        {
            TgvImage oldTgv = BytesToTgv(orginalImageContent);
            TgvImage newtgv = DDSFileToTgv(sourceImagePath, !Command.UseMipMaps);

            ImageComposerService.ReplaceImageTile(oldTgv, newtgv, (uint)Command.TileSize.Value, (uint)Command.Column.Value, (uint)Command.Row.Value);

            byte[] rawOldTgv = TgvToBytes(oldTgv, !Command.UseMipMaps);
            return rawOldTgv;
        }

    }

}
