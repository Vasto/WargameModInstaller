using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstaller.Common.Entities
{
    /// <summary>
    /// The base class for an enum like classes specifying PathType.
    /// </summary>
    public abstract class PathType : Enumeration
    {
        protected PathType(int value, String name)
            : base(value, name)
        {

        }

    }

}
