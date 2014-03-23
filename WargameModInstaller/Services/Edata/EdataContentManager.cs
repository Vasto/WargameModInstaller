using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Services.Edata
{
    //Dodać jeszcze jakiąś funkcjonalnośc odpowiadajaca ze ocene czy plik nadaje sie do monitorowanie.
    //coś typu minimlany wyamgany rozmiar aby zacząć go monitorować
    public class EdataContentManager
    {
        public EdataContentManager(EdataFile file) : this(file, 0)
        {
        }

        public EdataContentManager(EdataFile file, long maxLoadedContentBytes)
        {
            this.MenagedFile = file;
            this.MaxLoadedContentBytes = maxLoadedContentBytes;

            if(true /*if is total size of content files can be bigger then maxLoadedContentBytes */)
            {
                RegisterForContentLoadedEvent(file.ContentFiles);
            }
        }

        public event EventHandler MaxLoadedContentReached;

        public EdataFile MenagedFile
        {
            get;
            private set;
        }

        public long MaxLoadedContentBytes
        {
            get;
            private set;
        }

        public bool IsMaxLoadedContentReached()
        {
            long totalLoadedContentBytes = 0;
            foreach (var cf in MenagedFile.ContentFiles)
            {
                if (cf.IsContentLoaded)
                {
                    totalLoadedContentBytes += cf.Content.LongLength;
                }

                if (totalLoadedContentBytes >= MaxLoadedContentBytes)
                {
                    return true;
                }
            }

            return false;
        }

        public void FreeLoadedContent()
        {
            foreach (var cf in MenagedFile.ContentFiles)
            {
                if (cf.IsContentLoaded)
                {
                    cf.Content = null;
                }
            }
        }

        private void NotifyMaxLoadedContentReached()
        {
            var handler = MaxLoadedContentReached;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        private void RegisterForContentLoadedEvent(IEnumerable<EdataContentFile> contentFiles)
        {
        }

    }
}
