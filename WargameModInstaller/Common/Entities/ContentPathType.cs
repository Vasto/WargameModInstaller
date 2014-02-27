using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstaller.Common.Entities
{
    /// <summary>
    /// Represents the possible types of content paths.
    /// </summary>
    public class ContentPathType : PathType
    {
        public static readonly ContentPathType EdataContent = new ContentPathType(1, "EdataContent");
        public static readonly ContentPathType EdataNestedContent = new ContentPathType(2, "EdataNestedContent");
        public static readonly ContentPathType Unknown = new ContentPathType(3, "Unknown");

        protected ContentPathType(int value, String name)
            : base(value, name)
        {

        }

    }
}
