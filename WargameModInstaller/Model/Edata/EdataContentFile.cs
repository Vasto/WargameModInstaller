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
    ///struct dictFileEntry {
    ///     DWORD groupId;
    ///     DWORD fileEntrySize;
    ///     DWORD offset;
    ///     DWORD chunk2;   
    ///     DWORD fileSize;
    ///     DWORD chunk4;
    ///     blob checksum[16];
    ///     zstring name;
    /// };
    /// </remarks>
    public class EdataContentFile : EdataContentEntity
    {
        public string Path
        {
            get;
            set; 
        }

        /// <summary>
        /// Offset względem początku częsci danych.
        /// </summary>
        public long Offset
        {
            get;
            set; 
        }

        /// <summary>
        /// Offset uwzględniający wartość ofsetu wskazywaną przez plik nagłówka.
        /// Offset względem początku pliku.
        /// </summary>
        public long TotalOffset
        {
            get;
            set; 
        }

        //Update w przypadku zmiany contentu
        public long Size
        {
            get;
            set; 
        }

        public byte[] Checksum
        {
            get;
            set; 
        }

        public uint Id
        {
            get;
            set; 
        }

        public EdataContentFileType FileType
        {
            get;
            set;
        }

        public byte[] Content
        {
            get;
            set;
        }

        //Zastanowić się nad tym jak to powinno być ustalane
        public bool IsContentLoaded
        {
            get
            {
                return Content != null;
            }
        }

    }

}
