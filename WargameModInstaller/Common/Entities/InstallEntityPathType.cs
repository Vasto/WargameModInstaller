using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstaller.Common.Entities
{
    /// <summary>
    /// Represents the possible types of Installation Entity paths.
    /// </summary>
    public class InstallEntityPathType : PathType
    {
        public static readonly InstallEntityPathType Absolute = new InstallEntityPathType(1, "Absolute");
        public static readonly InstallEntityPathType Relative = new InstallEntityPathType(2, "Relative");
        public static readonly InstallEntityPathType Unknown = new InstallEntityPathType(3, "Unknown");

        protected InstallEntityPathType(int value, String name)
            : base(value, name)
        {

        }

    }

}
