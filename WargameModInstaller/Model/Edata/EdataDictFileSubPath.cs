using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Edata
{
    public class EdataDictFileSubPath : EdataDictSubPath
    {
        ///// <summary>
        ///// Gets or sets the length of entry in bytes.
        ///// </summary>
        //public uint Length
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// Gets or sets the offset of the file relatively to the content area beginning.
        /// </summary>
        public long FileOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the length of the file content in bytes.
        /// </summary>
        public long FileLength
        {
            get;
            set;
        }

        /// <summary>
        ///  Gets or sets the file checksum
        /// </summary>
        public byte[] FileChecksum
        {
            get;
            set;
        }

        ///// <summary>
        ///// Gets or sets the part of the path hold by the dict entry.
        ///// </summary>
        //public String Name
        //{
        //    get;
        //    set;
        //}
    }
}
