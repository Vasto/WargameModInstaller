using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers;

namespace WargameModInstaller.Infrastructure.Containers
{
    public interface IContainerWriterService
    {
        void WriteFile(IContainerFile containerFile);
        void WriteFile(IContainerFile containerFile, CancellationToken token);
        byte[] WriteRaw(IContainerFile containerFile);
        byte[] WriteRaw(IContainerFile containerFile, CancellationToken token);
    }
}
