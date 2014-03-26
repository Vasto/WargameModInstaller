using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Image;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Services.Commands
{
    //To do: reconsider method names

    public abstract class ReplaceCmdExecutorBase<T> : CmdExecutorBase<T> where T : IInstallCmd
    {
        public ReplaceCmdExecutorBase(T command) : base(command)
        {

        }

        protected TgvImage GetTgvFromDDS(String sourceFullPath)
        {
            ITgvFileReader tgvReader = new TgvDDSReader();
            TgvImage newtgv = tgvReader.Read(sourceFullPath);

            return newtgv;
        }

        protected TgvImage GetTgvFromContent(EdataContentFile edataContentFile)
        {
            ITgvBinReader rawReader = new TgvBinReader();
            TgvImage oldTgv = rawReader.Read(edataContentFile.Content);

            return oldTgv;
        }

        protected byte[] ConvertTgvToBytes(TgvImage tgv)
        {
            ITgvBinWriter tgvRawWriter = new TgvNoMipMapBinWriter();
            var bytes = tgvRawWriter.Write(tgv);

            return bytes;
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
