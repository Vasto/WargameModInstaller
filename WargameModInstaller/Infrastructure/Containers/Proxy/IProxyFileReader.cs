using System;
using System.Collections.Generic;
using System.Threading;
using WargameModInstaller.Model.Containers.Proxy;

namespace WargameModInstaller.Infrastructure.Containers.Proxy
{
    public interface IProxyFileReader
    {
        ProxyFile Read(String path, bool loadContent = false);
        ProxyFile Read(String path, bool loadContent, CancellationToken token);
        void LoadContent(ProxyContentFile file);
        void LoadContent(IEnumerable<ProxyContentFile> files);
    }
}
