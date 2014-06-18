using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstaller.Common.Entities
{
    /// <summary>
    /// Represents a possible types of content paths.
    /// </summary>
    public class ContentPathType : PathType
    {
        public static readonly ContentPathType Normal = new ContentPathType(1, "Normal");
        public static readonly ContentPathType MultipleNested = new ContentPathType(2, "MultipleNested");
        public static readonly ContentPathType Unknown = new ContentPathType(3, "Unknown");

        protected ContentPathType(int value, String name)
            : base(value, name)
        {

        }

    }
}
