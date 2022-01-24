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
        public static readonly WargameVersionType RedDragon = new WargameVersionType(1, "RD", "Wargame: Red Dragon");
        public static readonly WargameVersionType AirLandBattle = new WargameVersionType(2, "ALB", "Wargame: AirLand Battle");
        public static readonly WargameVersionType EuropeanEscalation = new WargameVersionType(3, "EE", "Wargame: European Escalation");
        public static readonly WargameVersionType WARNO = new WargameVersionType(1, "WN", "WARNO");

        protected static HashSet<WargameVersionType> knownVersions;

        static WargameVersionType()
        {
            knownVersions = new HashSet<WargameVersionType>();
            knownVersions.Add(RedDragon);
            knownVersions.Add(AirLandBattle);
            knownVersions.Add(EuropeanEscalation);
            knownVersions.Add(WARNO);
        }

        public static bool IsKnownVersion(String versionName)
        {
            return knownVersions.Any(v => v.Name == versionName);
        }

        public static WargameVersionType GetByName(String versionName)
        {
            return knownVersions.Single(v => v.Name == versionName);
        }

        public static WargameVersionType GetDefault()
        {
            return AirLandBattle;
        }

        protected WargameVersionType(int value, String name, String fullName)
            : base(value, name)
        {
            this.FullName = fullName;
        }

        public String FullName
        {
            get;
            protected set;
        }

    }
}
