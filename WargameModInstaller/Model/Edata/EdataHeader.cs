using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WargameModInstaller.Model.Edata
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// Thanks to Wargame:EE DAT Unpacker by Giovanni Condello
    /// struct edataHeader
    /// {
    ///	    CHAR edat[4];
    ///	    blob junk[21];
    ///	    DWORD dirOffset;
    ///	    DWORD dirLength;
    ///	    DWORD fileOffset;
    ///	    DWORD fileLength;
    /// };
    /// </remarks>
    public class EdataHeader
    {
        public int Version 
        { 
            get; 
            set; 
        }

        public byte[] Checksum 
        {
            get; 
            set; 
        }

        public int DirOffset 
        { 
            get; 
            set; 
        }

        public int DirLengh 
        { 
            get; 
            set; 
        }

        public int FileOffset
        {
            get;
            set;
        }

        public int FileLengh 
        {
            get; 
            set;
        }

    }
}
