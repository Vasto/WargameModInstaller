using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    /// <summary>
    /// Represents a commands which uses a source file embedded inside another file.
    /// </summary>
    public interface IHasNestedSource
    {
        /// <summary>
        /// Gets or sets a path of the nested source file residing inside the target file.
        /// </summary>
        ContentPath NestedSourcePath { get; set; }
    }
}
