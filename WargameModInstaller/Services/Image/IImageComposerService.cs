using System;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Services.Image
{
    public interface IImageComposerService
    {
        void ReplaceImageTile(TgvImage destination, TgvImage source, uint tileSize, uint column, uint row);
        void ReplaceImagePart(TgvImage destination, TgvImage source, uint xPos, uint yPos);
    }
}
