using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Infrastructure.Content;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Services.Commands
{
    public class AddContentCmdExecutor : EdataTargetCmdExecutorBase<AddContentCmd>
    {
        public AddContentCmdExecutor(AddContentCmd command)
            : base(command)
        {
            this.TotalSteps = 1;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null)
        {
            CurrentStep = 0;
            CurrentMessage = Command.GetExecutionMessage();

            //Cancel if requested;
            token.ThrowIfCanceledAndNotNull();

            String sourceFullPath = Command.SourcePath.GetAbsoluteOrPrependIfRelative(context.InstallerSourceDirectory);
            if (!File.Exists(sourceFullPath))
            {
                throw new CmdExecutionFailedException(
                    String.Format("Command's source file \"{0}\" doesn't exist.", Command.SourcePath),
                    String.Format(Properties.Resources.AddContentErrorParamMsg, Command.SourcePath));
            }

            String targetfullPath = Command.TargetPath.GetAbsoluteOrPrependIfRelative(context.InstallerTargetDirectory);
            if (!File.Exists(targetfullPath))
            {
                throw new CmdExecutionFailedException(
                    String.Format("Command's target file \"{0}\" doesn't exist.", Command.TargetPath),
                    String.Format(Properties.Resources.AddContentErrorParamMsg, Command.SourcePath));
            }

            //Może trzeba spr czy to jest poprawna ścieżka contentu? 
            //Bo jeśli to będzie taka która przysporzy kłopotów to rozwali cały plik.
            String contentPath = Command.NestedTargetPath.LastPart;
            if (contentPath == null)
            {
                throw new CmdExecutionFailedException(
                    String.Format("Invalid value of command's TargetContentPath: \"{0}\"", Command.NestedTargetPath),                   
                    String.Format(Properties.Resources.AddContentErrorParamMsg, Command.SourcePath));
            }

            //Uniezależnić to od konkretnych typów kontenerów
            var edataFileReader = new EdataFileReader();
            var contentOwningEdata = CanGetEdataFromContext(context) ?
                GetEdataFromContext(context) :
                edataFileReader.Read(targetfullPath, false);

            if (!contentOwningEdata.ContainsContentFileWithPath(contentPath))
            {
                var newContentFile = new EdataContentFile();
                newContentFile.Path = contentPath;
                newContentFile.Content = (new ContentFileReader()).Read(sourceFullPath); ;

                contentOwningEdata.AddContentFile(newContentFile);
            }
            else if (Command.OverwriteIfExist)
            {
                var contentFile = contentOwningEdata.GetContentFileByPath(contentPath);
                //if (!contentFile.IsContentLoaded)
                //{
                //    edataFileReader.LoadContent(contentFile);
                //}

                contentFile.Content = (new ContentFileReader()).Read(sourceFullPath);
            }

            //This needs more appropriate name
            if (!CanGetEdataFromContext(context))
            {
                SaveEdataFile(contentOwningEdata, token);
            }

            CurrentStep = TotalSteps;   
        }

    }
}
