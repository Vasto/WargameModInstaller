using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Infrastructure.Image;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Services.Commands
{
    public class AddImageCmdExecutor : EdataTargetCmdExecutorBase<AddImageCmd>
    {
        public AddImageCmdExecutor(AddImageCmd command)
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
                var image = DDSFileToTgv(sourceFullPath);
                //Zastanowic sie czy to nie odczytywac z kąds...
                image.IsCompressed = true; 
                //Trzeba mieć na to oko, czy nie powoduje problemów, bo przy replace była używana checksuma starego obrazka.
                image.SourceChecksum = image.ComputeContentChecksum(); 

                var newContentFile = new EdataContentFile();
                newContentFile.Path = contentPath;
                newContentFile.Content = TgvToBytes(image);

                contentOwningEdata.AddContentFile(newContentFile);
            }
            else if (Command.OverwriteIfExist)
            {
                var contentFile = contentOwningEdata.GetContentFileByPath(contentPath);
                if (!contentFile.IsContentLoaded)
                {
                    edataFileReader.LoadContent(contentFile);
                }

                if (contentFile.FileType != EdataContentFileType.Image)
                {
                    throw new CmdExecutionFailedException(
                        "Invalid command's TargetContentPath value. It doesn't point to an image content.",
                        String.Format(Properties.Resources.ReplaceImageErrorParametrizedMsg, Command.SourcePath));
                }

                TgvImage oldTgv = BytesToTgv(contentFile.Content);
                TgvImage newtgv = DDSFileToTgv(sourceFullPath, !Command.UseMipMaps);
                newtgv.SourceChecksum = oldTgv.SourceChecksum;
                newtgv.IsCompressed = oldTgv.IsCompressed;

                contentFile.Content = TgvToBytes(newtgv, !Command.UseMipMaps);
            }

            //This needs more appropriate name
            if (!CanGetEdataFromContext(context))
            {
                SaveEdataFile(contentOwningEdata, token);
            }

            CurrentStep = TotalSteps;
        }

        //Tu mamy powielenie, wiec trzeba pomyśleć o jakiejś wspólnej bazie z komendami podmieniajacymi obrazy.
        protected TgvImage DDSFileToTgv(String sourceFullPath, bool discardMipMaps = true)
        {
            ITgvFileReader tgvReader = discardMipMaps ?
                (ITgvFileReader)(new TgvDDSMoMipMapsReader()) :
                (ITgvFileReader)(new TgvDDSReader());

            TgvImage newtgv = tgvReader.Read(sourceFullPath);

            return newtgv;
        }

        protected TgvImage BytesToTgv(byte[] rawTgv)
        {
            ITgvBinReader rawReader = new TgvBinReader();
            TgvImage oldTgv = rawReader.Read(rawTgv);

            return oldTgv;
        }

        protected byte[] TgvToBytes(TgvImage tgv, bool discardMipMaps = true)
        {
            ITgvBinWriter tgvRawWriter = discardMipMaps ?
                new TgvBinNoMipMapsWriter() :
                new TgvBinWriter();

            var bytes = tgvRawWriter.Write(tgv);

            return bytes;
        }

    }
}
