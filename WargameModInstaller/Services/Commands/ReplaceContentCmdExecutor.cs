using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Content;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Services.Commands.Base;

namespace WargameModInstaller.Services.Commands
{
    public class ReplaceContentCmdExecutor : ModImageBySourceCmdExecutor<ReplaceContentCmd>
    {
        public ReplaceContentCmdExecutor(ReplaceContentCmd command)
            : base(command)
        {
            this.DefaultExecutionErrorMsg = String.Format(
                Properties.Resources.ReplaceContentErrorParamMsg, 
                Command.SourcePath);
        }

        protected override void ExecuteCommandsLogic(CmdsExecutionData data)
        {
            var contentFile = data.ContainerFile.GetContentFileByPath(data.ContentPath);
            var content = (new ContentFileReader()).Read(data.ModificationSourcePath);
            contentFile.LoadCustomContent(content);
        }

    }

}
