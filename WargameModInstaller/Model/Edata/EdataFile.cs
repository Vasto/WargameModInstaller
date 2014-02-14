using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WargameModInstaller.Model.Edata
{
    public class EdataFile
    {
        public EdataFile(
            String path, 
            EdataHeader header, 
            IEnumerable<EdataContentFile> contentFiles)
        {
            this.Path = path;
            this.Header = header;
            this.ContentFiles = contentFiles;
        }

        public String Path 
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

        public IEnumerable<EdataContentFile> ContentFiles
        {
            get;
            private set;
        }

    }
}
