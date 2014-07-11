using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers.Proxy
{
    public class ProxyFileTableEntry
    {
        public static readonly uint EntryLength = 24;

        public ProxyFileTableEntry()
        {
            this.Hash = new byte[8];
        }

        public byte[] Hash
        {
            get;
            set;
        }

        public uint Offset
        {
            get;
            set;
        }

        public uint Length
        {
            get;
            set;
        }

        public uint Unknown
        {
            get;
            set;
        }

        public uint Padding
        {
            get;
            set;
        }


    }
}
