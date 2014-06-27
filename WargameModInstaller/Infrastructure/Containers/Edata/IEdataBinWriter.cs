using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    public interface IEdataBinWriter
    {
        byte[] Write(EdataFile edata);
        byte[] Write(EdataFile edata, CancellationToken token);
    }
}
