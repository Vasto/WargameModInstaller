using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WargameModInstaller.Model.Edata
{
    public class EdataFile
    {
        private IDictionary<String, EdataContentFile> contentFilesDictionary;

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
            this.contentFilesDictionary = contentFiles.ToDictionary(x => x.Path);
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
            this.contentFilesDictionary = contentFiles.ToDictionary(x => x.Path);
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

        /// <summary>
        /// Ca³oœæ danych pomiedzy ostatnim bajtem nag³ówka, a pierwszym bajtem contentu plików.
        /// W sumie to s¹ g³ównie zera, s³ownik, zera... (po co to tak naprawde by³a?, wiem ze niedzialo bez tego...)
        /// </summary>
        public byte[] PostHeaderData
        {
            get;
            private set;
        }

        public IEnumerable<EdataContentFile> ContentFiles
        {
            get
            {
                return contentFilesDictionary.Values;
            }
        }

        public EdataContentFile GetContentFileByPath(String contentPath)
        {
            EdataContentFile result;
            if (contentFilesDictionary.TryGetValue(contentPath, out result))
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException(
                    String.Format(Properties.Resources.ContentFileNotFoundParametrizedMsg, contentPath));
            }
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
