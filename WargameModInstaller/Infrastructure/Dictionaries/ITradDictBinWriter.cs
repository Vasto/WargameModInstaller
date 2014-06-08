using System;
using System.Collections.Generic;
using WargameModInstaller.Model.Dictionaries;

namespace WargameModInstaller.Infrastructure.Dictionaries
{
    public interface ITradDictBinWriter
    {
        byte[] Write(IEnumerable<TradDictEntry> entries);
    }
}
