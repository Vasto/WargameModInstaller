using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Infrastructure.Edata
{
    public interface IEdataWriter
    {
        void Write(EdataFile fileToWrite);
        void Write(EdataFile fileToWrite, CancellationToken token);
    }
}
