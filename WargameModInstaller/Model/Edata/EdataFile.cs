using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WargameModInstaller.Model.Edata
{
    public class EdataFile
    {
        /// <summary>
        /// Creates an instance of EdataFile which doesn't reefer to any physical Edata file.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="contentFiles"></param>
        public EdataFile(
            EdataHeader header,
            byte[] postHeaderData, 
            IEnumerable<EdataContentFile> contentFiles)
        {
            this.Header = header;
            this.PostHeaderData = postHeaderData;
            this.ContentFiles = contentFiles;
            this.IsVirtual = true;

            AssignOwnership(this.ContentFiles);
        }

        /// <summary>
        /// Creates an instance of EdataFile which represent physical EdataFile witha a file path. 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="header"></param>
        /// <param name="contentFiles"></param>
        public EdataFile(
            String path, 
            EdataHeader header,
            byte[] postHeaderData, 
            IEnumerable<EdataContentFile> contentFiles)
        {
            this.Path = path;
            this.Header = header;
            this.PostHeaderData = postHeaderData;
            this.ContentFiles = contentFiles;
            this.IsVirtual = false;

            AssignOwnership(this.ContentFiles);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// From now it might not have a path.
        /// </remarks>
        public String Path 
        {
            get; 
            private set; 
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsVirtual
        {
            get;
            private set;
        }

        public String Name
        {
            get
            {
                return System.IO.Path.GetFileName(Path);
            }
        }

        public EdataHeader Header
        {
            get;
            private set;
        }

        public byte[] PostHeaderData
        {
            get;
            private set;
        }

        public IEnumerable<EdataContentFile> ContentFiles
        {
            get;
            private set;
        }

        protected void AssignOwnership(IEnumerable<EdataContentFile> contentFiles)
        {
            foreach (var cf in contentFiles)
            {
                cf.Owner = this;
            }
        }

    }
}
