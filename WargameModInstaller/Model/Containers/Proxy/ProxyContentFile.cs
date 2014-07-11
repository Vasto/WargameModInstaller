using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers;

namespace WargameModInstaller.Model.Containers.Proxy
{
    public class ProxyContentFile : IContentFile
    {
        public ProxyContentFile()
        {
            this.Hash = new byte[0];
            this.FileType = ContentFileType.Image;
        }

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

        public uint TotalOffset
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

        /// <summary>
        /// Gets or sets the path in the Proxy container 
        /// </summary>
        public String Path
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

        /// <summary>
        /// Gets or sets the file raw content.
        /// </summary>
        public byte[] Content
        {
            get;
            private set;
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
        /// Gets the information wheather the file's content is loaded with user's custom data.
        /// </summary>
        public bool IsCustomContentLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the information wheather the file's content is loaded with orginal data.
        /// </summary>
        public bool IsOriginalContentLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Loads object content with the provided original data.
        /// </summary>
        /// <param name="data"></param>
        public void LoadOrginalContent(byte[] data)
        {
            Content = data;

            IsContentLoaded = true;
            IsOriginalContentLoaded = true;
            IsCustomContentLoaded = false;

            NotifyContentLoaded();
        }

        /// <summary>
        /// Loads object content with the provided user's custom data.
        /// </summary>
        /// <param name="data"></param>
        public void LoadCustomContent(byte[] data)
        {
            Content = data;

            IsContentLoaded = true;
            IsCustomContentLoaded = true;
            IsOriginalContentLoaded = false;

            NotifyContentLoaded();
        }

        /// <summary>
        /// Discards object's current content;
        /// </summary>
        public void UnloadContent()
        {
            Content = null;

            IsContentLoaded = false;
            IsCustomContentLoaded = false;
            IsOriginalContentLoaded = false;

            NotifyContentUnloaded();
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", BitConverter.ToString(Hash), Path);
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
