using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers;

namespace WargameModInstaller.Services.Containers
{
    public class ContainerFileLoadManager
    {
        private readonly static long defaultMaxLoad = 838860800; //800MB

        private IList<IContentFile> managedFiles;

        public ContainerFileLoadManager()
            : this(new IContentFile[] { }, defaultMaxLoad) 
        {
        }

        public ContainerFileLoadManager(IEnumerable<IContentFile> contentFiles)
            : this(contentFiles, defaultMaxLoad)
        {
        }

        public ContainerFileLoadManager(IContainerFile containerFile)
            : this(containerFile.ContentFiles)
        {
        }

        public ContainerFileLoadManager(IContainerFile containerFile, long maxLoadInBytes)
            : this(containerFile.ContentFiles, maxLoadInBytes)
        {
        }

        public ContainerFileLoadManager(IEnumerable<IContentFile> contentFiles, long maxLoadInBytes)
        {
            this.managedFiles = contentFiles.ToList();
            this.MaxLoad = maxLoadInBytes;

            //This is not an ideal solution, because the total size of the loaded content can change
            //when assigning a bigger new content, not just loading old one... Though it only can cause problems on borderline situations
            //where the possilbe max size of loaded content is quite close to the maxLoadedContentBytes.
            //long possilbeTotalSizeOfLoadedContent = DeterminePotentialMaxLoad(contentFiles);
            //if (possilbeTotalSizeOfLoadedContent > maxLoadInBytes)
            //{
            RegisterForContentLoadedEvent(contentFiles);
            //}
        }

        public event EventHandler MaxLoadReached;

        public long MaxLoad
        {
            get;
            private set;
        }

        public IEnumerable<IContentFile> MenagedFiles
        {
            get { return managedFiles; }
        }

        public void AddManagedFile(IContentFile file)
        {
            AddManagedFiles(new IContentFile[] { file });
        }

        public void AddManagedFiles(IEnumerable<IContentFile> files)
        {
            // Może zamienić to na jakieś bardziej precyzyjne sprawdzanie jak np.
            // takie gdzie można dodać tylko jeden plik kontentu o danej ścieżce dla danego ownera.

            //Choć z drugiej strony to powinno wystarczyć, bo bierząca klasa działa w obrębie jednego egzekutora
            //który operuja na jednej instacji pliku kontenerowego, tak więc nie będzie możliwości pozyskania
            //drugiego obiektu pliku kontentu, reprezentujacego ten sam fiziczyny plik o tej samej ścieżce.
            foreach (var file in files)
            {
                if (!managedFiles.Contains(file))
                {
                    managedFiles.Add(file);

                    RegisterForContentLoadedEvent(file);

                    //Policzyć czy osiągnięto max load. W takiej sytuacji trzeba zrobić chyba
                    //utrzymywanie stanu biezacego loadu, zeby nie przeliczać tego za kazdym razem od nowa.
                }
            }
        }

        public void RemoveManagedFile(IContentFile file)
        {
            if (managedFiles.Contains(file))
            {
                UnregisterFromContentLoadedEvent(file);

                managedFiles.Remove(file);
            }
        }

        public void RemoveAllManagedFiles()
        {
            UnregisterFromContentLoadedEvent(managedFiles);

            managedFiles.Clear();
        }

        public bool IsMaxLoadReached()
        {
            long totalLoadedContentBytes = 0;
            foreach (var cf in MenagedFiles)
            {
                if (cf.IsContentLoaded)
                {
                    totalLoadedContentBytes += cf.ContentSize;
                }

                if (totalLoadedContentBytes >= MaxLoad)
                {
                    return true;
                }
            }

            return false;
        }

        public void FreeManagedFilesLoad()
        {
            foreach (var cf in MenagedFiles)
            {
                if (cf.IsContentLoaded)
                {
                    cf.Content = null;
                }
            }
        }

        private void NotifyMaxLoadReached()
        {
            var handler = MaxLoadReached;
            if (handler != null)
            {
                handler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Determines a possible total size of a content in bytes in the given content files collection.
        /// </summary>
        /// <param name="contentFiles"></param>
        /// <returns></returns>
        //private long DeterminePotentialMaxLoad(IEnumerable<IContentFile> contentFiles)
        //{
        //    long totalSize = 0;
        //    foreach (var cf in contentFiles)
        //    {
        //        totalSize += (cf.IsContentLoaded ? cf.ContentSize : cf.Size);
        //    }

        //    return totalSize;
        //}

        private void RegisterForContentLoadedEvent(IContentFile contentFile)
        {
            RegisterForContentLoadedEvent(new IContentFile[] { contentFile });
        }

        private void RegisterForContentLoadedEvent(IEnumerable<IContentFile> contentFiles)
        {
            foreach (var cf in contentFiles)
            {
                cf.ContentLoaded += ManagedFileContentLoadedHandler;
            }
        }

        private void UnregisterFromContentLoadedEvent(IContentFile contentFile)
        {
            RegisterForContentLoadedEvent(new IContentFile[] { contentFile });
        }

        private void UnregisterFromContentLoadedEvent(IEnumerable<IContentFile> contentFiles)
        {
            foreach (var cf in contentFiles)
            {
                cf.ContentLoaded -= ManagedFileContentLoadedHandler;
            }
        }

        private void ManagedFileContentLoadedHandler(object sender, EventArgs e)
        {
            if(IsMaxLoadReached())
            {
                var handler = MaxLoadReached;
                if (handler != null)
                {
                    handler(this, new EventArgs());
                }
            }
        }


    }
}
