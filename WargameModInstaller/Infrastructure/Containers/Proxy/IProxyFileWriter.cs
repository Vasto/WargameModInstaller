using System;
using System.Threading;
using WargameModInstaller.Model.Containers.Proxy;

namespace WargameModInstaller.Infrastructure.Containers.Proxy
{
    public interface IProxyFileWriter
    {
        void Write(ProxyFile file);
        void Write(ProxyFile file, CancellationToken token);
    }
}
