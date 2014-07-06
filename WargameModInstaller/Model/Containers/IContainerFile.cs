using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers
{
    //To do: zastanowić się czy to nie powinna zawierać także właściwości określającej typ pliku kontenera.

    public interface IContainerFile
    {
        /// <summary>
        /// Gets or sets the path of the container file.
        /// </summary>
        String Path { get; set; }

        /// <summary>
        /// Gets the collection of content files belonging to the container file.
        /// </summary>
        IReadOnlyCollection<IContentFile> ContentFiles { get; }

        /// <summary>
        /// Adds a given content file to the container file.
        /// </summary>
        /// <param name="file"></param>
        void AddContentFile(IContentFile file);

        /// <summary>
        /// Removes a specified content file from the container file.
        /// </summary>
        /// <param name="file"></param>
        void RemoveContentFile(IContentFile file);

        /// <summary>
        /// Gets a content file with the specified content path.
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        IContentFile GetContentFileByPath(String contentPath);

        /// <summary>
        /// Checks whether a content file with a specified content files belongs to the conatiner file.
        /// </summary>
        /// <param name="contentPath"></param>
        /// <returns></returns>
        bool ContainsContentFileWithPath(String contentPath);
    }
}
