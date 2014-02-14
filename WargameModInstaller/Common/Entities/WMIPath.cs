using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Common.Entities
{
    /// <summary>
    /// The Wargame Mod Installer cutom path data type. 
    /// It holds additional helper info about the path type.
    /// </summary>
    public class WMIPath : IEquatable<WMIPath>
    {
        public static implicit operator String(WMIPath path)
        {
            return path.Value;
        }

        public WMIPath(String pathValue, WMIPathType pathType)
        {
            //1. Zrobić sprawdzania poprawności formy podanej ścieżki, zacząć od resource...
            //  pewnie można by porównać czy istnieje w: Assembly.GetExecutingAssembly().GetManifestResourceNames();
            //2.Jakis algorytm sam rozstrzygający PathType

            this.PathType = pathType;
            this.Value = pathValue;
        }

        /// <summary>
        /// Gets information about the current path type.
        /// </summary>
        public WMIPathType PathType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the path value.
        /// </summary>
        public String Value
        {
            get;
            private set;
        }

        public bool Equals(WMIPath other)
        {
            WMIPath otherPath = other as WMIPath;
            if (otherPath != null)
            {
                return (otherPath.Value == this.Value) &&
                    (otherPath.PathType == this.PathType);
            }
            else
            {
                return false;
            }
        }

        public override bool Equals(object obj)
        {
            WMIPath other = obj as WMIPath;
            if (other != null)
            {
                return Equals(other);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode() +
                PathType.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }

    }

    /// <summary>
    /// Represents a different path types, like relative or absolute.
    /// </summary>
    public enum WMIPathType
    {
        Relative,
        Absolute,
        EmbeddedResource
    }

}
