using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers;

namespace WargameModInstaller.Infrastructure.Containers
{
    public interface IContainerReaderService
    {
        IContainerFile ReadFile(String containerFilePath, bool loadContent);
        IContainerFile ReadFile(String containerFilePath, bool loadContent, CancellationToken token);
        IContainerFile ReadRaw(byte[] rawContainerFile, bool loadContent);
        IContainerFile ReadRaw(byte[] rawContainerFile, bool loadContent, CancellationToken token);
        void LoadContent(IContentFile file);
        void LoadContent(IEnumerable<IContentFile> files);
    }
}
