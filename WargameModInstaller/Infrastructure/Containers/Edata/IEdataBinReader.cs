﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers.Edata;

namespace WargameModInstaller.Infrastructure.Containers.Edata
{
    public interface IEdataBinReader
    {
        EdataFile Read(byte[] rawProxy, bool loadContent);
        EdataFile Read(byte[] rawProxy, bool loadContent, CancellationToken token);
    }
}
