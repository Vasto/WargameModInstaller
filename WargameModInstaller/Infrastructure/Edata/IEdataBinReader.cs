using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Infrastructure.Edata
{
    public interface IEdataBinReader
    {
        EdataFile Read(byte[] rawEdata, bool loadContent);
        EdataFile Read(byte[] rawEdata, bool loadContent, CancellationToken token);
    }
}
