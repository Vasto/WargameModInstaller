using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Model.Image;

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
                    String.Format(WargameModInstaller.Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
            }


            String rootContentPath = Command.TargetContentPath.Split().FirstOrDefault();
            if (rootContentPath == null)
            {
                throw new CmdExecutionFailedException(
                    "Invalid command's TargetContentPath value.",
                    String.Format(WargameModInstaller.Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
            }


            var edataReader = new EdataFileReader();
            var mainEdataFile = CanGetEdataFromContext(context) ?
                GetEdataFromContext(context) :
                edataReader.Read(targetfullPath, false); //Wprowadzić to wszędzie, najlepiej w formie metod klasy bazowe

            //First one contnet is from the edata on hdd, so needs to be loaded explicitly.
            EdataContentFile rootContentFile = GetEdataContentFileByPath(mainEdataFile, rootContentPath);
            if (!rootContentFile.IsContentLoaded)
            {
                edataReader.LoadContent(rootContentFile);
            }


            //Prepare a list of nested content according to the given paths.
            var contentFilesList = GetContentFilesHierarchy(mainEdataFile, Command.TargetContentPath.Split());


            var imageContentFile = contentFilesList.Last();
            if (imageContentFile.FileType != EdataContentFileType.Image)
            {
                throw new CmdExecutionFailedException(
                    "Invalid command's TargetContentPath value. It doesn't point to an image content.",
                    String.Format(WargameModInstaller.Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
            }

            CurrentStep++;

            TgvImage oldTgv = GetTgvFromContent(imageContentFile);
            TgvImage newtgv = GetTgvFromDDS(sourceFullPath);
            newtgv.SourceChecksum = oldTgv.SourceChecksum;
            newtgv.IsCompressed = oldTgv.IsCompressed;

            byte[] rawNewTgv = ConvertTgvToBytes(newtgv);


            //Assign content changes for all packages, from bottom to top
            AssignContentUpHierarchy(contentFilesList, rawNewTgv);

            if (!CanGetEdataFromContext(context))
            {
                IEdataFileWriter edataWriter = new EdataFileWriter();
                if (token.HasValue)
                {
                    edataWriter.Write(mainEdataFile, token.Value);
                }
                else
                {
                    edataWriter.Write(mainEdataFile);
                }
            }

            CurrentStep = TotalSteps;
        }

    }

}
