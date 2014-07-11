using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Containers.Proxy
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ProxyHeader
    {
        public ulong Magic;
        //Always 8
        public uint Version;
        public uint FileLength;
        //It seems to be not related to the content of file table, path table nor the files content...
        //It also doesn't change between updates...
        public Md5Hash Checksum;
        public uint FileTableOffset;
        public uint FileTableLength;
        public uint ContentOffset;
        public uint ContentLength;
        //This could be swapped with PathTableEntriesCount,
        //not sure about the order as those two always have the same value for all proxy files
        public uint FileTableEntriesCount; 
        public uint PathTableOffset;
        public uint PathTableLength;
        public uint PathTableEntriesCount;
    }

}
