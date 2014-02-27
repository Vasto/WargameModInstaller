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
    public abstract class PathBase : IEquatable<PathBase>
    {
        public static implicit operator String(PathBase path)
        {
            return path.Value;
        }

        private IEnumerable<PathTypeResolveRule> resolveRules;

        /// <summary>
        /// Creates a path representation based on the given path and type.
        /// </summary>
        /// <param name="pathValue"></param>
        /// <param name="pathType"></param>
        public PathBase(String pathValue)
        {
            this.ResolveRules = CreateResolveRules();
            this.PathType = ResolvePathType(pathValue);
            this.Value = pathValue;
        }

        /// <summary>
        /// Creates a path representation based on the given path and type.
        /// </summary>
        /// <param name="pathValue"></param>
        /// <param name="pathType"></param>
        public PathBase(String pathValue, PathType pathType)
        {
            this.ResolveRules = CreateResolveRules();
            this.PathType = pathType;
            this.Value = pathValue;
        }

        /// <summary>
        /// Gets the stored path.
        /// </summary>
        public String Value
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets information about the current path type.
        /// </summary>
        public PathType PathType
        {
            get;
            private set;
        }

        protected IEnumerable<PathTypeResolveRule> ResolveRules
        {
            get;
            private set;
        }

        public bool Equals(PathBase other)
        {
            PathBase otherPath = other as PathBase;
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
            PathBase other = obj as PathBase;
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

        protected abstract IEnumerable<PathTypeResolveRule> CreateResolveRules();
        protected abstract PathType GetDefaultPathType();

        protected virtual PathType ResolvePathType(String path)
        {
            var orderedRules = ResolveRules.OrderBy(x => x.Priority);
            foreach (var rule in orderedRules)
            {
                bool matchesRule = rule.Resolve(path);
                if (matchesRule)
                {
                    return rule.Type;
                }
            }

            return GetDefaultPathType();
        }

        #region PathTypeResolveRule

        protected class PathTypeResolveRule
        {
            public PathTypeResolveRule(PathType type, int priority, Func<String, bool> rule)
            {
                this.Type = type;
                this.Resolve = rule;
                this.Priority = priority;
            }

            public int Priority { get; private set; }
            public PathType Type { get; private set; }
            public Func<String, bool> Resolve { get; private set; }

        }

        #endregion //PathTypeResolveRule

    }

}
