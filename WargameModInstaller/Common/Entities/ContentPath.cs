using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstaller.Common.Entities
{
    /// <summary>
    /// Represents a path to a specific content residing inside a dat file.
    /// </summary>
    public class ContentPath : PathBase
    {
        public ContentPath(String pathValue)
            : base(pathValue)
        {

        }

        public ContentPath(String pathValue, ContentPathType pathType)
            : base(pathValue, pathType)
        {
        }

        /// <summary>
        /// Splits the path into sub paths if available.
        /// </summary>
        /// <returns></returns>
        public string[] Split()
        {
            var subPaths = Array.ConvertAll(
                Value.Split(new[] { "|+" }, StringSplitOptions.RemoveEmptyEntries), 
                p => p.Trim());

            return subPaths;
        }

        protected override IEnumerable<PathTypeResolveRule> CreateResolveRules()
        {
            var edataMultilevelContentPathRule = new PathTypeResolveRule(ContentPathType.EdataNestedContent, 1, EdataMultilevelContentPathRule);
            var edataContentPathRule = new PathTypeResolveRule(ContentPathType.EdataContent, 2, EdataContentPathRule);

            var rulesList = new List<PathTypeResolveRule>();
            rulesList.Add(edataMultilevelContentPathRule);
            rulesList.Add(edataContentPathRule);

            return rulesList;
        }

        protected override PathType GetDefaultPathType()
        {
            return ContentPathType.Unknown;
        }

        private bool EdataMultilevelContentPathRule(String path)
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

        private bool EdataContentPathRule(String path)
        {
            return PathUtilities.IsValidRelativePath(path);
        }

    }
}
