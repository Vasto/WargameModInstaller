using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstaller.Common.Entities
{
    /// <summary>
    /// Represents a path to an application resource.
    /// </summary>
    public class ResourcePath : PathBase
    {
        public ResourcePath(String pathValue)
            : base(pathValue)
        {

        }

        public ResourcePath(String pathValue, ResourcePathType pathType)
            : base(pathValue, pathType)
        {

        }

        protected override IEnumerable<PathTypeResolveRule> CreateResolveRules()
        {
            var embededResourcePathRule = new PathTypeResolveRule(ResourcePathType.EmbeddedResource, 1, EmbeddedResourceResolveRule);
            var absolutePathRule = new PathTypeResolveRule(ResourcePathType.LocalAbsolute, 2, AbsolutePathRule);
            var relativePathRule = new PathTypeResolveRule(ResourcePathType.LocalRelative, 3, RelativePathRule);

            var rulesList = new List<PathTypeResolveRule>();
            rulesList.Add(embededResourcePathRule);
            rulesList.Add(absolutePathRule);
            rulesList.Add(relativePathRule);

            return rulesList;
        }

        protected override PathType GetDefaultPathType()
        {
            return ResourcePathType.Unknown;
        }

        private bool EmbeddedResourceResolveRule(String path)
        {
            var resourceNames = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();

            return resourceNames.Contains(path);
        }

        private bool AbsolutePathRule(String path)
        {
            return PathUtilities.IsValidAbsolutePath(path);
        }

        private bool RelativePathRule(String path)
        {
            return PathUtilities.IsValidRelativePath(path);
        }

    }
}
