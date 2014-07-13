using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers.Proxy;

namespace WargameModInstaller.Infrastructure.Containers.Proxy
{
    public interface IProxyBinReader
    {
        ProxyFile Read(byte[] rawProxy, bool loadContent = true);
        ProxyFile Read(byte[] rawProxy, bool loadContent, CancellationToken token);
    }
}
