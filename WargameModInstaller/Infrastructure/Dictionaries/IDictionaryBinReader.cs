using System;
using System.Collections.Generic;
using WargameModInstaller.Model.Dictionaries;

namespace WargameModInstaller.Infrastructure.Dictionaries
{
    public interface IDictionaryBinReader
    {
        IEnumerable<DictionaryEntry> Read(byte[] rawDictionaryData);
    }
}
