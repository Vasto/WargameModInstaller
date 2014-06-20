using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Image;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Services.Commands.Base
{
    public abstract class ModImageBySourceCmdExecutor<T> : ModNestedTargetBySourceCmdExecutor<T>
        where T : IInstallCmd, IHasTarget, IHasNestedTarget, IHasSource
    {
        public ModImageBySourceCmdExecutor(T command)
            : base(command)
        {

        }

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
