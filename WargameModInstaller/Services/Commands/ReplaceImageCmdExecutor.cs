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
    /// <summary>
    /// 
    /// </summary>
    public class ReplaceImageCmdExecutor : ReplaceImageCmdExecutorBase<ReplaceImageCmd>
    {
        public ReplaceImageCmdExecutor(ReplaceImageCmd command)
            : base(command)
        {

        }

        protected override byte[] ModifyImageContent(byte[] orginalImageContent, String sourceImagePath)
        {
            TgvImage oldTgv = GetTgvFromBytes(orginalImageContent);
            TgvImage newtgv = GetTgvFromDDS(sourceImagePath, !Command.UseMipMaps);
            newtgv.SourceChecksum = oldTgv.SourceChecksum;
            newtgv.IsCompressed = oldTgv.IsCompressed;

            byte[] rawNewTgv = ConvertTgvToBytes(newtgv, !Command.UseMipMaps);
            return rawNewTgv;
        }
    }

}
