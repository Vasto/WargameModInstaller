using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Config
{
    public class WargameVersionType : Enumeration
    {
        public static readonly WargameVersionType RedDragon = new WargameVersionType(1, "RD");
        public static readonly WargameVersionType AirLandBattle = new WargameVersionType(2, "ALB");
        public static readonly WargameVersionType EuropeanEscalation = new WargameVersionType(3, "EE");

        protected static HashSet<WargameVersionType> knownVersions;

        static WargameVersionType()
        {
            knownVersions = new HashSet<WargameVersionType>();
            knownVersions.Add(RedDragon);
            knownVersions.Add(AirLandBattle);
            knownVersions.Add(EuropeanEscalation);
        }

        public static bool IsKnownVersion(String versionName)
        {
            return knownVersions.Any(v => v.Name == versionName);
        }

        public static WargameVersionType GetDefault()
        {
            return AirLandBattle;
        }

        protected WargameVersionType(int value, String name)
            : base(value, name)
        {

        }

    }
}
