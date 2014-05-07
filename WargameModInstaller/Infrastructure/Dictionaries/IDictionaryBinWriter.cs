using System;
using System.Collections.Generic;
using WargameModInstaller.Model.Dictionaries;

namespace WargameModInstaller.Infrastructure.Dictionaries
{
    public interface IDictionaryBinWriter
    {
        byte[] Write(IEnumerable<DictionaryEntry> entries);
    }
}
