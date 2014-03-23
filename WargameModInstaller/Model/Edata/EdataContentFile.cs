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
        private byte[] content;

        /// <summary>
        /// Occurs when the content is loaded
        /// </summary>
        public event EventHandler ContentLoaded;

        /// <summary>
        /// Occurs when the content is unloaded
        /// </summary>
        public event EventHandler ContentUnloaded;

        public String Path
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

        /// <summary>
        /// Rozmiar pierwotny, odczytany, modyfikowane kiedy nastepuje powrotny zapis.
        /// </summary>
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
            get
            {
                return content;
            }
            set
            {
                content = value;
                if (content != null)
                {
                    IsContentLoaded = true;
                    NotifyContentLoaded();
                }
                else
                {
                    IsContentLoaded = false;
                    NotifyContentUnloaded();
                }
            }
        }

        public long ContentSize
        {
            get
            {
                return IsContentLoaded ? Content.Length : 0;
            }
        }

        //Zastanowić się nad tym jak to powinno być ustalane
        public bool IsContentLoaded
        {
            get;
            private set;
        }

        private void NotifyContentLoaded()
        {
            var handler = ContentLoaded;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void NotifyContentUnloaded()
        {
            var handler = ContentUnloaded;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

    }

}
