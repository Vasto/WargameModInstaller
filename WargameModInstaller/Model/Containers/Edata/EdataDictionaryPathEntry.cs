using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers.Edata
{
    /// <summary>
    /// Represents a hierarchical entry in the EDATa's file dictionary which holds a part of the content's path.
    /// </summary>
    public abstract class EdataDictionaryPathEntry
    {
        protected List<EdataDictionaryPathEntry> followingEntries;

        public EdataDictionaryPathEntry(String pathPart)
        {
            if (pathPart == null)
            {
                throw new ArgumentNullException("Edata dictionary entry cannot have null path.", "subPath");
            }

            this.PathPart = pathPart;
            this.followingEntries = new List<EdataDictionaryPathEntry>();
        }

        /// <summary>
        /// Gets or sets the parent entry in the hierarchy.
        /// </summary>
        public EdataDictionaryPathEntry PrecedingEntry
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the children dictionary entries.
        /// </summary>
        public IReadOnlyList<EdataDictionaryPathEntry> FollowingEntries
        {
            get
            {
                return followingEntries;
            }
        }

        /// <summary>
        /// Gets the path fragment hold by the current entry.
        /// </summary>
        public String PathPart
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the full path of the entry, 
        /// which is combined path of current entry and all preceeding entries.
        /// </summary>
        public String FullPath
        {
            get
            {
                return GetFullPath();
            }
        }

        /// <summary>
        /// Gets or sets the length of entry in bytes.
        /// </summary>
        public int Length
        {
            get
            {
                return GetLengthInBytes();
            }
        }

        public virtual EdataDictionaryPathEntry SelectEntryByPath(String path)
        {
            EdataDictionaryPathEntry result = null;

            //This alghoritm assumes that there are no two child entries with the same sub paths.

            if (path == PathPart)
            {
                result = this;
            }
            else if (path.StartsWith(PathPart))
            {
                int startIndex = PathPart.Length;
                EdataDictionaryPathEntry currentMatch = this;
                bool matchFound = true;
                while (matchFound)
                {
                    matchFound = false;

                    for (int i = 0; i < currentMatch.FollowingEntries.Count; ++i)
                    {
                        var potentialMatch = currentMatch.FollowingEntries[i];
                        var subPath = potentialMatch.PathPart;

                        if ((startIndex < path.Length) &&
                            (startIndex + subPath.Length <= path.Length) &&
                            (path.IndexOf(subPath, startIndex, subPath.Length) != -1))
                        {
                            currentMatch = potentialMatch;
                            matchFound = true;
                            startIndex += subPath.Length;
                            break;
                        }
                    }

                    if (matchFound && startIndex == path.Length)
                    {
                        result = currentMatch;
                        break;
                    }
                }
            }

            return result;
        }

        public virtual IEnumerable<T> SelectEntriesOfType<T>() where T : EdataDictionaryPathEntry
        {
            var entries = new List<T>();

            foreach (var entry in FollowingEntries)
            {
                if (entry is T)
                {
                    entries.Add((T)entry);
                }

                entries.AddRange(entry.SelectEntriesOfType<T>());
            }

            return entries;
        }

        public virtual bool IsEndingEntry()
        {
            if (PrecedingEntry != null)
            {
                var lastSubPath = PrecedingEntry.FollowingEntries.LastOrDefault();
                if (lastSubPath != null && lastSubPath == this)
                {
                    return true;
                }
            }

            return false;
        }

        public override String ToString()
        {
            return PathPart.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        public abstract byte[] ToBytes();

        protected String GetFullPath()
        {
            StringBuilder path = new StringBuilder();

            var currentEntry = this;
            while (currentEntry != null)
            {
                path.Insert(0, currentEntry.PathPart);

                currentEntry = currentEntry.PrecedingEntry;
            }

            return path.ToString();
        }

        protected abstract int GetLengthInBytes();

    }
}
