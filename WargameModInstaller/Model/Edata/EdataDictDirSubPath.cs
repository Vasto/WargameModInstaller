using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Edata
{
    /// <summary>
    /// Represents the Edata file's dictionary entry.
    /// </summary>
    public class EdataDictDirSubPath : EdataDictSubPath
    {
        ///// <summary>
        ///// Gets or sets the length of entry in bytes.
        ///// </summary>
        //public uint Length
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// Gets or sets the length in bytes of the dictionary area 
        /// where the current entry is relevant for other entries.
        /// </summary>
        public uint RelevanceLength
        {
            get;
            set;
        }

        ///// <summary>
        ///// Gets or sets the path part of the current entry.
        ///// </summary>
        //public String Content
        //{
        //    get;
        //    set;
        //}

        public void AddFollowingSubPath(EdataDictSubPath path)
        {
            if (!followingSubPaths.Contains(path))
            {
                followingSubPaths.Add(path);
            }
        }

        public void RemoveFollowingSubPath(EdataDictSubPath path)
        {
            if (followingSubPaths.Contains(path))
            {
                followingSubPaths.Remove(path);
            }
        }

        public void RemoveAllFollowingSubPaths()
        {
            followingSubPaths.Clear();
        }

    }
}
