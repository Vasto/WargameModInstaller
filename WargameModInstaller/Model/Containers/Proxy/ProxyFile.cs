using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Containers;

namespace WargameModInstaller.Model.Containers.Proxy
{
    //Co ciekawe spieprzenie pliku proxy nie skutkuje crashami 
    //a jedynie brakiem textur proxy przy przełaczaniu tekstur i użyciem gołego modelu.

    public class ProxyFile : IContainerFile
    {
        private IDictionary<String, IContentFile> contentFilesDictionary;

        public ProxyFile(ProxyHeader header, IEnumerable<ProxyContentFile> contentFiles)
            : this (null, header, contentFiles)
        {

        }

        public ProxyFile(String path, ProxyHeader header, IEnumerable<ProxyContentFile> contentFiles)
        {
            this.Path = path;
            this.Header = header;
            this.contentFilesDictionary = contentFiles
                .OfType<IContentFile>()
                .ToDictionary(x => x.Path);

            AssignOwnership(contentFiles);
        }

        /// <summary>
        /// 
        /// </summary>
        public ProxyHeader Header
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path of the container file.
        /// </summary>
        public String Path
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the collection of content files belonging to the container file.
        /// </summary>
        public IReadOnlyCollection<IContentFile> ContentFiles
        {
            get
            {
                return contentFilesDictionary.Values.ToList();
            }
        }

        /// <summary>
        /// Gets a content file with the specified content path.
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        public IContentFile GetContentFileByPath(String contentPath)
        {
            IContentFile result;
            if (contentFilesDictionary.TryGetValue(contentPath, out result))
            {
                return result;
            }
            else
            {
                throw new InvalidOperationException();
                //throw new InvalidOperationException(
                //    String.Format(WargameModInstaller.Properties.Resources.ContentFileNotFoundParamMsg, contentPath));
            }
        }

        /// <summary>
        /// Checks whether a content file with a specified content files belongs to the conatiner file.
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        public bool ContainsContentFileWithPath(String path)
        {
            if (String.IsNullOrEmpty(path))
            {
                throw new ArgumentException(
                    String.Format("Cannot vaerify existance of the file without the specified content path."),
                    "contentFile");
            }

            return contentFilesDictionary.ContainsKey(path);
        }

        /// <summary>
        /// Adds a given content file to the container file.
        /// </summary>
        /// <param name="file"></param>
        public void AddContentFile(IContentFile contentFile)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes a specified content file from the container file.
        /// </summary>
        /// <param name="file"></param>
        public void RemoveContentFile(IContentFile contentFile)
        {
            throw new NotImplementedException();
        }

        protected void AssignOwnership(IEnumerable<IContentFile> contentFiles)
        {
            foreach (var cf in contentFiles)
            {
                cf.Owner = this;
            }
        }

    }
}
