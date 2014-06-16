using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Edata
{
    //To do: pomyśleć nad nazwą tej klasy

    /// <summary>
    /// Represents the Edata file's dictionary entry.
    /// </summary>
    public class EdataDictDirSubPath : EdataDictSubPath
    {
        public EdataDictDirSubPath(String subPath) : base(subPath)
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

        public override byte[] ToBytes()
        {
            //Dodać nowy obiekt pojedyńczego roota który będzie miał nadpisana tą metoda tak by zwrócić 00000000 dla relevance.
            byte[] buffer;
            using (var ms = new MemoryStream())
            {
                buffer = BitConverter.GetBytes(Length);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(RelevanceLength);
                ms.Write(buffer, 0, buffer.Length);

                buffer = Encoding.ASCII.GetBytes(SubPath);
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
            uint relevance = GetTotalLengthInBytes();
            foreach (var entry in FollowingSubPaths)
            {
                if (entry is EdataDictDirSubPath)
                {
                    relevance += ((EdataDictDirSubPath)entry).RelevanceLength;
                }
                else
                {
                    relevance += entry.TotalLength;
                }
            }

            return relevance;
        }

        protected override uint GetTotalLengthInBytes()
        {
            uint totalLength = 0;

            //Length
            totalLength += sizeof(uint);

            //RelevanceLength
            totalLength += sizeof(uint);

            //SubPath
            totalLength += (uint)SubPath.Length;

            //zero ended string
            totalLength += 1;

            //supplement to even length.
            totalLength += (uint)(totalLength % 2 == 0 ? 0 : 1);

            return totalLength;
        }

        protected override uint GetLengthInBytes()
        {
            return GetTotalLengthInBytes();
        }


    }
}
