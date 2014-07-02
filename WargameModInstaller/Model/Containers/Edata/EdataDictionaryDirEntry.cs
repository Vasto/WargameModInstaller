using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers.Edata
{
    //To do: pomyśleć nad nazwą tej klasy

    /// <summary>
    /// Represents the Edata file's dictionary entry.
    /// </summary>
    public class EdataDictionaryDirEntry : EdataDictionaryPathEntry
    {
        public EdataDictionaryDirEntry(String pathPart)
            : base(pathPart)
        {
           
        }

        /// <summary>
        /// Gets or sets the length in bytes of the dictionary area 
        /// where the current entry is relevant for other entries.
        /// //To też liczyć z potomków.
        /// </summary>
        public uint RelevanceLength
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
            }
        }

        public void RemoveFollowingEntry(EdataDictionaryPathEntry entry)
        {
            if (followingEntries.Contains(entry))
            {
                followingEntries.Remove(entry);
            }
        }

        public override byte[] ToBytes()
        {
            byte[] buffer;
            using (var ms = new MemoryStream())
            {
                buffer = BitConverter.GetBytes(Length);
                ms.Write(buffer, 0, buffer.Length);

                uint relevancelength = IsPathEndingEntry() ? 0 : RelevanceLength;
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

        protected virtual uint GetRelevanceLength()
        {
            uint relevance = Length;
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

        protected override uint GetLengthInBytes()
        {
            uint totalLength = 0;

            //Length
            totalLength += sizeof(uint);

            //RelevanceLength
            totalLength += sizeof(uint);

            //SubPath
            totalLength += (uint)PathPart.Length;

            //zero ended string
            totalLength += 1;

            //supplement to even length.
            totalLength += (uint)(totalLength % 2 == 0 ? 0 : 1);

            return totalLength;
        }

    }
}
