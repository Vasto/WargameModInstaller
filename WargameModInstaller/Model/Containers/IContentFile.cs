using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers
{
    public interface IContentFile
    {
        /// <summary>
        /// Gets or sets the content owner container file.
        /// </summary>
        IContainerFile Owner { get; set; }

        /// <summary>
        /// Gets or sets the path of the content file in the container file.
        /// </summary>
        String Path { get; set; }

        /// <summary>
        /// Gets or sets the content file type.
        /// </summary>
        ContentFileType FileType { get; set; }

        /// <summary>
        /// Gets or sets the file raw content.
        /// </summary>
        byte[] Content { get; set; }

        /// <summary>
        /// Gets the information wheather the file's content is loaded.
        /// </summary>
        bool IsContentLoaded { get; }

        /// <summary>
        /// Gets the file's content length in bytes.
        /// </summary>
        long ContentSize { get; }

        /// <summary>
        /// Occurs when the content is loaded
        /// </summary>
        event EventHandler ContentLoaded;

        /// <summary>
        /// Occurs when the content is unloaded
        /// </summary>
        event EventHandler ContentUnloaded;

    }
}
