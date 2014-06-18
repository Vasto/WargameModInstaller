using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstaller.Common.Entities
{
    /// <summary>
    /// Represents the possible types of application resource paths.
    /// </summary>
    public class ResourcePathType : PathType
    {
        public static readonly ResourcePathType LocalAbsolute = new ResourcePathType(1, "LocalAbsolute");
        public static readonly ResourcePathType LocalRelative = new ResourcePathType(2, "LocalRelative");
        public static readonly ResourcePathType Embedded = new ResourcePathType(3, "Embedded");
        public static readonly ResourcePathType Unknown = new ResourcePathType(3, "Unknown");

        protected ResourcePathType(int value, String name)
            : base(value, name)
        {

        }

    }

}
