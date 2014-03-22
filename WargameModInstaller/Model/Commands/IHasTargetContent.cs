using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    public interface IHasTargetContent
    {
        /// <summary>
        /// Gets or sets path inside the target file to the command's target content.
        /// </summary>
        ContentPath TargetContentPath { get; set; }
    }
}
