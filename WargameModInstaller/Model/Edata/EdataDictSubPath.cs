using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Edata
{
    /// <summary>
    /// Represents a hierarchical entry in the EDATa's file dictionary.
    /// </summary>
    public abstract class EdataDictSubPath
    {
        protected List<EdataDictSubPath> followingSubPaths;

        public EdataDictSubPath()
        {
            this.followingSubPaths = new List<EdataDictSubPath>();
        }

        /// <summary>
        /// Gets or sets the parent entry in the hierarchy.
        /// </summary>
        public EdataDictDirSubPath PrecedingSubPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the children dictionary entries.
        /// </summary>
        public IReadOnlyList<EdataDictSubPath> FollowingSubPaths
        {
            get
            {
                return followingSubPaths;
            }
        }

        /// <summary>
        /// Gets or sets the length of entry in bytes.
        /// </summary>
        public uint Length
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the textual content of the current dictioanry entry.
        /// </summary>
        public String SubPath
        {
            get;
            set;
        }

        //wyniesc to z tad bo to no chyba ze pominiemy koniecznosc odnoszenia sie do pliku czyli podklasy bo to jest zło
        //Wywalić to pozniej fo extension method, wtedy sobie moze operowac pojeciem pliku
        public EdataDictSubPath SelectEntryByPath(String path)
        {
            EdataDictSubPath result = null;

            //Działanie tego algorytmu zakłąda, 
            //że nie mogą istnieć 2 ścieżki o tych samych SubPathach, dla tego samego rodzica.
            if (path == SubPath)
            {
                result = this;
            }
            else if (path.StartsWith(SubPath))
            {
                int startIndex = SubPath.Length;
                var currentMatch = this;
                bool matchFound = true;
                while (matchFound)
                {
                    matchFound = false;

                    for (int i = 0; i < currentMatch.followingSubPaths.Count; ++i)
                    {
                        var potentialMatch = currentMatch.followingSubPaths[i];
                        var subPath = potentialMatch.SubPath;

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
            return SubPath.ToString(System.Globalization.CultureInfo.CurrentCulture);
        }

    }
}
