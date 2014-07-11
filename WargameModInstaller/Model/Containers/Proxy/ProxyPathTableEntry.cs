using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers.Proxy
{
    public class ProxyPathTableEntry
    {
        public static readonly uint EntryLength = 264;

        public ProxyPathTableEntry()
        {
            this.Hash = new byte[8];
        }

        public String Path
        {
            get;
            set;
        }

        public byte[] Hash
        {
            get;
            set;
        }
    }
}
