using System;
using System.Threading;
using WargameModInstaller.Model.Containers.Proxy;

namespace WargameModInstaller.Infrastructure.Containers.Proxy
{
    interface IProxyBinWriter
    {
        byte[] Write(ProxyFile file);
        byte[] Write(ProxyFile file, CancellationToken token);
    }
}
