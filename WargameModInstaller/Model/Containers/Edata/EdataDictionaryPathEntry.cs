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
        public EdataDictionaryPathEntry PrecedingEntries
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
        /// Gets or sets the path fragment hold by the current entry.
        /// </summary>
        public String PathPart
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the length of entry in bytes.
        /// </summary>
        public uint Length
        {
            get
            {
                return GetLengthInBytes();
            }
        }

        //wyniesc to z tad bo to no chyba ze pominiemy koniecznosc odnoszenia sie do pliku czyli podklasy bo to jest zło
        //Wywalić to pozniej fo extension method, wtedy sobie moze operowac pojeciem pliku
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
                var currentMatch = this;
                bool matchFound = true;
                while (matchFound)
                {
                    matchFound = false;

                    for (int i = 0; i < currentMatch.followingEntries.Count; ++i)
                    {
                        var potentialMatch = currentMatch.followingEntries[i];
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

        public override String ToString()
        {
            return PathPart.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

        public abstract byte[] ToBytes();

        protected abstract uint GetLengthInBytes();

        protected virtual bool IsPathEndingEntry()
        {
            if (PrecedingEntries != null)
            {
                var lastSubPath = PrecedingEntries.FollowingEntries.LastOrDefault();
                if (lastSubPath != null && lastSubPath == this)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
