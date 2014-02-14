using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WargameModInstaller.Common.Extensions;

namespace WargameModInstaller.Services.Utilities
{
    /// <summary>
    /// Service to get the directory location.
    /// </summary>
    public class DirectoryLocationService : IDirectoryLocationService
    {
        private readonly FolderBrowserDialog directoryDialog;

        /// <summary>
        /// Initializes a new instance of the DirectoryLocationService class;
        /// </summary>
        public DirectoryLocationService()
        {
            this.directoryDialog = new FolderBrowserDialog();
        }

        /// <summary>
        /// Gets a selected directory path.
        /// </summary>
        public String SelectedDirectoryPath
        {
            get
            {
                return directoryDialog.SelectedPath;
            }
        }

        /// <summary>
        /// Gets or sets the initial directory for the location browsing;
        /// </summary>
        public Environment.SpecialFolder InitialDirectory
        {
            get
            {
                return directoryDialog.RootFolder;
            }
            set
            {
                directoryDialog.RootFolder = value;
            }
        }

        /// <summary>
        /// Determines the directory location;
        /// </summary>
        /// <returns></returns>
        public bool DetermineDirectoryLocation()
        {
            return directoryDialog.ShowDialog().ToBool();
        }

    }
}
