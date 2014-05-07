using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Infrastructure.Image;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Services.Commands
{
    public abstract class ReplaceImageCmdExecutorBase<T> : AlterEdataCmdExecutorBase<T> 
        where T : IInstallCmd, IHasSource, IHasTarget, IHasTargetContent
    {
        public ReplaceImageCmdExecutorBase(T command)
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
                edataFileReader.Read(targetfullPath, false);


            EdataContentFile contentFile = contentOwningEdata.GetContentFileByPath(contentPath);
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

            CurrentStep++;

            var modifiedImageContent = ModifyImageContent(contentFile.Content, sourceFullPath);
            contentFile.Content = modifiedImageContent;

            if (!CanGetEdataFromContext(context))
            {
                SaveEdataFile(contentOwningEdata, token);
            }

            CurrentStep = TotalSteps;
        }

        protected abstract byte[] ModifyImageContent(byte[] orginalImageContent, String sourceImagePath);

        protected TgvImage GetTgvFromDDS(String sourceFullPath, bool discardMipMaps = true)
        {
            ITgvFileReader tgvReader = discardMipMaps ?
                (ITgvFileReader)(new TgvDDSMoMipMapsReader()) :
                (ITgvFileReader)(new TgvDDSReader());

            TgvImage newtgv = tgvReader.Read(sourceFullPath);

            return newtgv;
        }

        protected TgvImage GetTgvFromBytes(byte[] rawTgv)
        {
            ITgvBinReader rawReader = new TgvBinReader();
            TgvImage oldTgv = rawReader.Read(rawTgv);

            return oldTgv;
        }

        protected byte[] ConvertTgvToBytes(TgvImage tgv, bool discardMipMaps = true)
        {
            ITgvBinWriter tgvRawWriter = discardMipMaps ?
                new TgvBinNoMipMapsWriter() :
                new TgvBinWriter();

            var bytes = tgvRawWriter.Write(tgv);

            return bytes;
        }

    }

}
