using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Content;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Containers.Edata;
using WargameModInstaller.Services.Commands.Base;

namespace WargameModInstaller.Services.Commands
{
    public class AddContentCmdExecutor : ModNestedTargetBySourceCmdExecutor<AddContentCmd>
    {
        public AddContentCmdExecutor(AddContentCmd command)
            : base(command)
        {
            this.DefaultExecutionErrorMsg = String.Format(
                Properties.Resources.AddContentErrorParamMsg, 
                Command.SourcePath);
        }

        protected override void ExecuteCommandsLogic(CmdsExecutionData data)
        {
            if (!data.ContainerFile.ContainsContentFileWithPath(data.ContentPath))
            {
                var newContentFile = new EdataContentFile();
                newContentFile.Path = data.ContentPath;

                var content = (new ContentFileReader()).Read(data.ModificationSourcePath);
                newContentFile.LoadCustomContent(content);

                data.ContainerFile.AddContentFile(newContentFile);
            }
            else if (Command.OverwriteIfExist)
            {
                var contentFile = data.ContainerFile.GetContentFileByPath(data.ContentPath);
                var content = (new ContentFileReader()).Read(data.ModificationSourcePath);
                contentFile.LoadCustomContent(content);
            }
        }

    }
}
