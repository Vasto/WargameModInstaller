using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Dictionaries
{
    /// <remarks>
    /// Credits go to enohka for this code.
    /// See more at: https://github.com/enohka/moddingSuite/blob/master/moddingSuite/Model/Trad/TradEntry.cs
    /// </remarks>
    public class TradDictEntry
    {
        public byte[] Hash
        {
            get;
            set;
        }

        public uint OffsetDictionary
        {
            get;
            set;
        }

        public uint OffsetContent
        {
            get;
            set;
        }

        public uint ContentLength
        {
            get;
            set;
        }

        public String Content
        {
            get;
            set;
        }

    }
}
