using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Infrastructure.Edata
{
    public interface IEdataBinWriter
    {
        byte[] Write(EdataFile edata);
        byte[] Write(EdataFile edata, CancellationToken token);
    }
}
