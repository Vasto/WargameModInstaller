using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers.Edata
{
    /// <summary>
    /// Represents the Edata file's dictionary entry.
    /// </summary>
    public class EdataDictionaryDirEntry : EdataDictionaryPathEntry
    {
        private int customLength;
        private int customRelevance;
        private bool useAutocalculatedValues;

        public EdataDictionaryDirEntry(String pathPart)
            : base(pathPart)
        {
            this.useAutocalculatedValues = true;
        }

        /// <summary>
        /// Creates a new EdataDictionaryFileEntry, with custom relevance and entryLength values.
        /// </summary>
        /// <param name="pathPart"></param>
        /// <param name="relevance"></param>
        public EdataDictionaryDirEntry(String pathPart, int entryLength, int relevance)
            : base(pathPart)
        {
            this.customRelevance = relevance;
            this.useAutocalculatedValues = false;
            this.customLength = entryLength;
        }

        /// <summary>
        /// Gets or sets the length in bytes of the dictionary area 
        /// where the current entry is relevant for other entries.
        /// </summary>
        public int RelevanceLength
        {
            get
            {
                return GetRelevanceLength();
            }
        }

        public void AddFollowingEntry(EdataDictionaryPathEntry entry)
        {
            if (!followingEntries.Contains(entry))
            {
                followingEntries.Add(entry);

                entry.PrecedingEntry = this;
            }
        }

        public void RemoveFollowingEntry(EdataDictionaryPathEntry entry)
        {
            if (followingEntries.Contains(entry))
            {
                followingEntries.Remove(entry);

                entry.PrecedingEntry = null;
            }
        }

        public override bool IsEndingEntry()
        {
            if (useAutocalculatedValues)
            {
                return base.IsEndingEntry();
            }
            else
            {
                return (customRelevance == 0);
            }
        }

        public override byte[] ToBytes()
        {
            byte[] buffer;
            using (var ms = new MemoryStream())
            {
                buffer = BitConverter.GetBytes(Length);
                ms.Write(buffer, 0, buffer.Length);

                int relevancelength = IsEndingEntry() ? 0 : RelevanceLength;
                buffer = BitConverter.GetBytes(relevancelength);
                ms.Write(buffer, 0, buffer.Length);

                buffer = Encoding.ASCII.GetBytes(PathPart);
                ms.Write(buffer, 0, buffer.Length);

                buffer = new byte[1];
                ms.Write(buffer, 0, buffer.Length);

                //Supplement to evenness
                if (ms.Length % 2 != 0)
                {
                    buffer = new byte[1];
                    ms.Write(buffer, 0, buffer.Length);
                }

                return ms.ToArray();
            }
        }

        protected virtual int GetRelevanceLength()
        {
            if (useAutocalculatedValues)
            {
                int relevance = Length;
                foreach (var entry in FollowingEntries)
                {
                    if (entry is EdataDictionaryDirEntry)
                    {
                        relevance += ((EdataDictionaryDirEntry)entry).RelevanceLength;
                    }
                    else
                    {
                        relevance += entry.Length;
                    }
                }

                return relevance;
            }
            else
            {
                return customRelevance;
            }
        }

        protected override int GetLengthInBytes()
        {
            if (useAutocalculatedValues)
            {
                int totalLength = 0;

                //Length
                totalLength += sizeof(uint);

                //RelevanceLength
                totalLength += sizeof(uint);

                //SubPath
                totalLength += PathPart.Length;

                //zero ended string
                totalLength += 1;

                //supplement to even length.
                totalLength += (totalLength % 2 == 0 ? 0 : 1);

                return totalLength;
            }
            else
            {
                return customLength;
            }
        }

    }
}
