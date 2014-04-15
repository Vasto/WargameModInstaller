using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Components
{
    public class ComponentType : Enumeration
    {
        public static readonly ComponentType Required = new ComponentType(1, "Required");
        public static readonly ComponentType Optional = new ComponentType(2, "Optional");
        public static readonly ComponentType Exclusive = new ComponentType(3, "Exclusive");

        protected static HashSet<ComponentType> knownTypes;

        static ComponentType()
        {
            knownTypes = new HashSet<ComponentType>();
            knownTypes.Add(Required);
            knownTypes.Add(Optional);
            knownTypes.Add(Exclusive);
        }

        public static bool IsKnownVersion(String typeName)
        {
            return knownTypes.Any(v => v.Name == typeName);
        }

        public static ComponentType GetByName(String typeName)
        {
            return knownTypes.Single(v => v.Name == typeName);
        }

        public static ComponentType GetDefault()
        {
            return Required;
        }

        protected ComponentType(int value, String name)
            : base(value, name)
        {

        }

    }
}
