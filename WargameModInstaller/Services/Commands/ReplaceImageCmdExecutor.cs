using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Model.Image;
using WargameModInstaller.Services.Commands.Base;

namespace WargameModInstaller.Services.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public class ReplaceImageCmdExecutor : ModImageBySourceCmdExecutor<ReplaceImageCmd>
    {
        public ReplaceImageCmdExecutor(ReplaceImageCmd command)
            : base(command)
        {
            this.DefaultExecutionErrorMsg = String.Format(
                Properties.Resources.ReplaceContentErrorParamMsg,
                Command.SourcePath);
        }

        protected override void ExecuteCommandsLogic(CmdsExecutionData data)
        {
            var contentFile = data.ContainerFile.GetContentFileByPath(data.ContentPath);
            if (!contentFile.IsContentLoaded)
            {
                (new EdataFileReader()).LoadContent(contentFile);
            }

            //To nie będzie potrzebne tutaj jeśli nie bedzie trzeba ładować i wykorzystywać starego obrazka.
            if (contentFile.FileType != EdataContentFileType.Image)
            {
                throw new CmdExecutionFailedException(
                    String.Format("Invalid TargetContentPath: \"{0}\". It doesn't target an image content file.", data.ContentPath),
                    DefaultExecutionErrorMsg);
            }

            //Ładowanie starego obrazka nie będzie potrzebne jeśli bedzie pewnośc ze recznie wygenerowana checkusma jest ok.
            TgvImage oldTgv = BytesToTgv(contentFile.Content);
            TgvImage newtgv = DDSFileToTgv(data.SourcePath, !Command.UseMipMaps);
            newtgv.SourceChecksum = oldTgv.SourceChecksum;
            newtgv.IsCompressed = oldTgv.IsCompressed;

            contentFile.Content = TgvToBytes(newtgv, !Command.UseMipMaps);
        }

    }

}
