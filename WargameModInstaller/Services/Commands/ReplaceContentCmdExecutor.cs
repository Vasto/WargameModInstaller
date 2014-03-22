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
    public class ReplaceContentCmdExecutor : ReplaceCmdExecutorBase<ReplaceContentCmd>
    {
        public ReplaceContentCmdExecutor(ReplaceContentCmd command)
            : base(command)
        {
            this.TotalSteps = 2;
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
                    String.Format(Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
            }


            String contentPath = Command.TargetContentPath.LastPart;
            if (contentPath == null)
            {
                throw new CmdExecutionFailedException(
                    "Invalid command's TargetContentPath value.",
                    String.Format(Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
            }

            var edataFileReader = new EdataFileReader();
            var contentOwningEdata = CanGetEdataFromContext(context) ?
                GetEdataFromContext(context) :
                edataFileReader.Read(targetfullPath, false); //Wprowadzić to wszędzie, najlepiej w formie metod klasy bazowe

            //First one contnet is from the edata on hdd, so needs to be loaded explicitly.
            EdataContentFile contentFile = GetEdataContentFileByPath(contentOwningEdata, contentPath);
            if (!contentFile.IsContentLoaded)
            {
                edataFileReader.LoadContent(contentFile);
            }

            CurrentStep++;

            var contentFileReader = new ContentFileReader();
            byte[] newContent = contentFileReader.Read(sourceFullPath);
            contentFile.Content = newContent;

            if (!CanGetEdataFromContext(context))
            {
                IEdataFileWriter edataWriter = new EdataFileWriter();
                if (token.HasValue)
                {
                    edataWriter.Write(contentOwningEdata, token.Value);
                }
                else
                {
                    edataWriter.Write(contentOwningEdata);
                }
            }

            CurrentStep = TotalSteps;
        }

    }

}
