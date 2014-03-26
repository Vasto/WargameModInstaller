using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Edata;

namespace WargameModInstaller.Services.Edata
{
    public class EdataContentManager
    {
        public EdataContentManager(IEnumerable<EdataContentFile> contentFiles)
            : this(contentFiles, 838860800) //800MB
        {
        }

        public EdataContentManager(EdataFile edataFile)
            : this(edataFile.ContentFiles)
        {
        }

        public EdataContentManager(IEnumerable<EdataContentFile> contentFiles, long maxLoadedContentBytes)
        {
            this.MenagedFiles = contentFiles;
            this.MaxLoadedContentBytes = maxLoadedContentBytes;

            //This is not an ideal solution, because the total siez of loaded content can change
            //when assigning a bigger new content, not just loading old one... Though it only can cause problems on borderline situations
            //where the possilbe max size of loaded content is quite close to the maxLoadedContentBytes.
            long possilbeTotalSizeOfLoadedContent = DeterminePossibleTotalSize(contentFiles);
            if (possilbeTotalSizeOfLoadedContent > maxLoadedContentBytes)
            {
                RegisterForContentLoadedEvent(contentFiles);
            }
        }

        public EdataContentManager(EdataFile edataFile, long maxLoadedContentBytes)
            : this (edataFile.ContentFiles, maxLoadedContentBytes)
        {
        }

        public event EventHandler MaxLoadedContentReached;

        public IEnumerable<EdataContentFile> MenagedFiles
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
            foreach (var cf in MenagedFiles)
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
            foreach (var cf in MenagedFiles)
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

        /// <summary>
        /// Determines a possible total size of a content in the given content files collectin.
        /// </summary>
        /// <param name="contentFiles"></param>
        /// <returns></returns>
        private long DeterminePossibleTotalSize(IEnumerable<EdataContentFile> contentFiles)
        {
            long totalSize = 0;
            foreach (var cf in contentFiles)
            {
                totalSize += (cf.IsContentLoaded ? cf.ContentSize : cf.Size);
            }

            return totalSize;
        }

        private void RegisterForContentLoadedEvent(IEnumerable<EdataContentFile> contentFiles)
        {
            foreach (var cf in contentFiles)
            {
                cf.ContentLoaded += ContentFileContentLoadedHandler;
            }
        }

        private void ContentFileContentLoadedHandler(object sender, EventArgs e)
        {
            if(IsMaxLoadedContentReached())
            {
                var handler = MaxLoadedContentReached;
                if (handler != null)
                {
                    handler(this, new EventArgs());
                }
            }
        }


    }
}
