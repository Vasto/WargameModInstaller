using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    /// <summary>
    /// Represents a commands which targets file embedded inside another file.
    /// </summary>
    public interface IHasNestedTarget
    {
        /// <summary>
        /// Gets or sets a path of the nested target file residing inside the target file.
        /// </summary>
        ContentPath NestedTargetPath { get; set; }

        /// <summary>
        /// Gets the information whether the command requires to work an orginal content of target. 
        /// </summary>
        bool UsesNestedTargetContent { get; }
    }
}
