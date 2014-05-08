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
    public class ReplaceImageTileCmdExecutor : ReplaceImageCmdExecutorBase<ReplaceImageTileCmd>
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
            TgvImage oldTgv = GetTgvFromBytes(orginalImageContent);
            TgvImage newtgv = GetTgvFromDDS(sourceImagePath, !Command.UseMipMaps);

            ImageComposerService.ReplaceImageTile(oldTgv, newtgv, (uint)Command.TileSize.Value, (uint)Command.Column.Value, (uint)Command.Row.Value);

            byte[] rawOldTgv = ConvertTgvToBytes(oldTgv, !Command.UseMipMaps);
            return rawOldTgv;
        }

    }

}
