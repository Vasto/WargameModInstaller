using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers.Edata
{
    //To do: pomyśleć nad nazwą tej klasy

    public class EdataDictFileSubPath : EdataDictSubPath
    {
        public EdataDictFileSubPath(String subPath) : base (subPath)
        {
            this.FileChecksum = new byte[16];
        }

        /// <summary>
        /// Gets or sets the offset of the file relatively to the content area beginning.
        /// </summary>
        public long FileOffset
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the length of the file content in bytes.
        /// </summary>
        public long FileLength
        {
            get;
            set;
        }

        /// <summary>
        ///  Gets or sets the file checksum.
        /// </summary>
        public byte[] FileChecksum
        {
            get;
            set;
        }

        public override byte[] ToBytes()
        {
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[4];
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(Length);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(FileOffset);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(FileLength);
                ms.Write(buffer, 0, buffer.Length);

                buffer = FileChecksum;
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

        protected override uint GetTotalLengthInBytes()
        {
            uint totalLength = 0;

            //FileHeader
            totalLength += 4;

            //Length
            totalLength += sizeof(uint);

            //FileOffset
            totalLength += sizeof(long);

            //FileLength
            totalLength += sizeof(long);

            //FileChecksum
            totalLength += (uint)FileChecksum.Length;

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
            uint length = GetTotalLengthInBytes();

            //chyba jednak jest ten nagłówek wliczony
            //length -= 4;

            //update: działa z tym nagłowkiem, czyli na dobra sprawe ta totalna długoś wpisu jest zbedna, bo ta normalna zwiera tą samą wartośc

            return length;
        }

    }
}
