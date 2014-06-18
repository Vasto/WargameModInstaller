using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstaller.Common.Entities
{
    /// <summary>
    /// Represents a path to a specific content residing inside a package file.
    /// </summary>
    public class ContentPath : PathBase
    {
        public ContentPath(String pathValue)
            : base(pathValue)
        {
            this.Parts = Split();
            this.PreLastPart = CreatePreLastPart();
        }

        public ContentPath(String pathValue, ContentPathType pathType)
            : base(pathValue, pathType)
        {
            this.Parts = Split();
        }

        /// <summary>
        /// Gets all sub paths of complex content path.
        /// </summary>
        public String[] Parts
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the last sub path.
        /// </summary>
        public String LastPart
        {
            get { return Parts.Last(); }
        }

        /// <summary>
        /// Gets all path parts except last one.
        /// When the cuurent path doesn't contain any additional sub paths it retruns null.
        /// </summary>
        public String PreLastPart
        {
            get;
            protected set;
        }

        protected String CreatePreLastPart()
        {
            if (Parts.Length > 1)
            {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < Parts.Length - 1; ++i)
                {
                    sb.Append(Parts[i]);
                }

                return sb.ToString();
            }
            else
            {
                return null;
            }
        }

        protected String[] Split()
        {
            return Array.ConvertAll(
                Value.Split(new[] { "|+" }, StringSplitOptions.RemoveEmptyEntries),
                p => p.Trim());
        }

        protected override IEnumerable<PathTypeResolveRule> CreateResolveRules()
        {
            var multipleNestedContentPathRule = new PathTypeResolveRule(
                ContentPathType.MultipleNested,
                1, 
                MultipleNestedPathRule);
            var normalContentPathRule = new PathTypeResolveRule(
                ContentPathType.Normal, 
                2, 
                NormalContentPathRule);

            var rulesList = new List<PathTypeResolveRule>();
            rulesList.Add(multipleNestedContentPathRule);
            rulesList.Add(normalContentPathRule);

            return rulesList;
        }

        protected override PathType GetDefaultPathType()
        {
            return ContentPathType.Unknown;
        }

        private bool MultipleNestedPathRule(String path)
        {
            var subPaths = path.Split(new[] { "|+" }, StringSplitOptions.RemoveEmptyEntries);
            if (subPaths.Length <= 1)
            {
                return false;
            }

            foreach (var item in subPaths)
            {
                if (!PathUtilities.IsValidRelativePath(item))
                {
                    return false;
                }
            }

            return true;
        }

        private bool NormalContentPathRule(String path)
        {
            return PathUtilities.IsValidRelativePath(path);
        }

    }
}
