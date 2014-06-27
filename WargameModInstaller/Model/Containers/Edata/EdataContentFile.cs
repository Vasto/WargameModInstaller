using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WargameModInstaller.Model.Containers.Edata
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
    public class EdataContentFile : EdataContentEntity, IContentFile
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

        /// <summary>
        /// Gets or sets the content owner container file.
        /// </summary>
        public IContainerFile Owner
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path written in the edata dictionary. 
        /// </summary>
        public String Path
        {
            get;
            set; 
        }

        /// <summary>
        /// Gets or sets the file raw content.
        /// </summary>
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

        /// <summary>
        /// Gets the file's content length in bytes.
        /// Returns zero when no content.
        /// 
        /// //Maxymalny rozmiar tablicy to int32.max, wiec to long jest zbędne...
        /// </summary>
        public long ContentSize
        {
            get
            {
                return IsContentLoaded ? Content.Length : 0;
            }
        }

        /// <summary>
        /// Gets the information wheather the file's content is loaded.
        /// False, when content null, true when content set, even if zero bytes long.
        /// </summary>
        public bool IsContentLoaded
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets or set the order id in the dictionary.
        /// </summary>
        public uint Id
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

        /// <summary>
        /// Gets or sets the file checksum.
        /// </summary>
        public byte[] Checksum
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the content file type.
        /// </summary>
        public ContentFileType FileType
        {
            get;
            set;
        }

        public override string ToString()
        {
            return Path;
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
