using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Containers.Edata
{
    /// <summary>
    /// Thanks to Wargame:EE DAT Unpacker by Giovanni Condello
    /// Restructured header
    /// struct edataHeader
    /// {
    /// DWORD magic;
    /// DWORD version;
    ///
    /// blob checksum_v1[16]; 
    /// blob skip[1]; 
    ///
    /// DWORD dictionaryOffset;  // ASM seems to say it must be 1037: CMP DWORD PTR DS:[ESI+0x19],0x40D   (Compare value at offset 25 is 1037)

    /// DWORD dictionaryLength;
    /// DWORD filesOffset;
    /// DWORD filesLength;
    ///
    /// DWORD unkown2; // always 0
    ///
    /// DWORD padding; // always 8192
    /// 
    /// blob checksum_v2[16];	
    /// };
    /// </summary>
    /// <remarks>
    /// Credits go to enohka for this code.
    /// See more at: http://github.com/enohka/moddingSuite
    /// </remarks>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct EdataHeader
    {
        public uint Magic;
        public uint Version;

        public Md5Hash Checksum_V1;
        public readonly byte Skip_1;

        public uint DictOffset;
        public uint DictLength;
        public uint FileOffset;
        public uint FileLenght;

        public readonly uint Unkown_1;
        //wyglada na to �e tutaj mamy zapian� liczbe kt�rej wielokrotno�� jest u�yta do oblicznenia offsetu cz�ci plik�w.
        public uint Padding; 

        public Md5Hash Checksum_V2;
    }

}
