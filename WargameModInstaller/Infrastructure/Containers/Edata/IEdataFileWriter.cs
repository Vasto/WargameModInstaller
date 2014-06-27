using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    public interface IEdataFileWriter
    {
        void Write(EdataFile fileToWrite);
        void Write(EdataFile fileToWrite, CancellationToken token);
    }
}
