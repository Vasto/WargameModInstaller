using System;
using System.Collections.Generic;
using WargameModInstaller.Model.Dictionaries;

namespace WargameModInstaller.Infrastructure.Dictionaries
{
    public interface ITradDictBinReader
    {
        IEnumerable<TradDictEntry> Read(byte[] rawDictionaryData);
    }
}
