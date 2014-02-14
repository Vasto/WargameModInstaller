using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Infrastructure.Image;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Services.Commands
{
    public abstract class ReplaceImageCmdExecutorBase<T> : CmdExecutorBase<T> where T : IInstallCmd
    {
        public ReplaceImageCmdExecutorBase(T command) : base(command)
        {

        }

        protected EdataContentFile GetEdataContentFile(EdataFile edataFile, String edataContentPath)
        {
            var edataContentFile = edataFile.ContentFiles.FirstOrDefault(f => f.Path == edataContentPath);
            if (edataContentFile == null)
            {
                throw new CmdExecutionFailedException(
                    String.Format("Cannot load \"{0}\"", edataContentPath),
                    WargameModInstaller.Properties.Resources.LoadEdataErrorMsg);
            }

            return edataContentFile;
        }

        protected EdataContentFile LoadEdataContentFile(EdataReader edataReader, EdataContentFile edataContentFile)
        {
            edataReader.LoadContent(edataContentFile);

            return edataContentFile;
        }

        protected TgvImage GetTgvFromDDS(String sourceFullPath)
        {
            ITgvReader tgvReader = new TgvDDSReader(sourceFullPath);
            TgvImage newtgv = tgvReader.Read();

            return newtgv;
        }

        protected TgvImage GetTgvFromEdataContent(EdataContentFile edataContentFile)
        {
            ITgvReader rawReader = new TgvRawReader(edataContentFile.Content);
            TgvImage oldTgv = rawReader.Read();

            return oldTgv;
        }

        protected byte[] ConvertTgvToBytes(TgvImage tgv)
        {
            using (var memoryStream = new MemoryStream())
            {
                ITgvWriter tgvRawWriter = new TgvNoMipMapRawStreamWriter(memoryStream);
                tgvRawWriter.Write(tgv);

                return memoryStream.ToArray();
            }
        }

        protected bool CanGetEdataFromContext(CmdExecutionContext context)
        {
            var sharedEdataContext = context as SharedEdataCmdExecutionContext;
            if (sharedEdataContext != null)
            {
                return sharedEdataContext.EdataFile != null;
            }
            else
            {
                return false;
            }
        }

        protected EdataFile GetEdataFromContext(CmdExecutionContext context)
        {
            var sharedEdataContext = context as SharedEdataCmdExecutionContext;
            if (sharedEdataContext != null)
            {
                return sharedEdataContext.EdataFile;
            }
            else
            {
                throw new InvalidOperationException("Cannot get Edata file from the given context");
            }
        }

    }

}
