using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers
{
    //Trzeba by mieć na uwadze teraz ze po zapisani do pliku content typu custom
    //powinine zostać oznaczony jako original... JEdnak z racji jednorazowaości 
    //wykorzystania pliku kontenru to nie stanowi problemu póki co.

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
        byte[] Content { get; }

        /// <summary>
        /// Gets the information wheather the file's content is loaded.
        /// </summary>
        bool IsContentLoaded { get; }

        /// <summary>
        /// Gets the information wheather the file's content is loaded with user's custom data.
        /// </summary>
        bool IsCustomContentLoaded { get; }

        /// <summary>
        /// Gets the information wheather the file's content is loaded with orginal data.
        /// </summary>
        bool IsOriginalContentLoaded { get; }

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

        /// <summary>
        /// Loads objects content with the provided original data.
        /// </summary>
        /// <param name="data"></param>
        void LoadOrginalContent(byte[] data);

        /// <summary>
        /// Loads objects content with the provided user's custom data.
        /// </summary>
        /// <param name="data"></param>
        void LoadCustomContent(byte[] data);

        /// <summary>
        /// Discards object's current content;
        /// </summary>
        void UnloadContent();

    }
}
