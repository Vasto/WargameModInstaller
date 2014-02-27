using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    //To do: Pomyśleć nad nazewnictwem tego wszystkiego.

    public interface IHasTarget
    {
        /// <summary>
        /// Gets or sets path inside the working directory to the command's target file.
        /// </summary>
        InstallEntityPath TargetPath
        {
            get;
            set;
        }

    }
}
