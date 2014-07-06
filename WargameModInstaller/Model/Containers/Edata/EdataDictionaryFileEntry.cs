using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Containers.Edata
{
    public class EdataDictionaryFileEntry : EdataDictionaryPathEntry
    {
        private int customLength;
        private bool useAutocalculatedValues;

        public EdataDictionaryFileEntry(String pathPart)
            : base(pathPart)
        {
            this.FileChecksum = new byte[16];
            this.useAutocalculatedValues = true;
        }

        public EdataDictionaryFileEntry(
            String pathPart, 
            long fileOffset, 
            long fileLength,
            byte[] fileChecksum)
            : base(pathPart)
        {
            this.FileOffset = fileOffset;
            this.FileLength = fileLength;
            this.FileChecksum = fileChecksum;
            this.useAutocalculatedValues = true;
        }

        /// <summary>
        /// Creates a new EdataDictionaryFileEntry, with custom file netry length value.
        /// </summary>
        /// <param name="pathPart"></param>
        /// <param name="length"></param>
        public EdataDictionaryFileEntry(String pathPart, int entryLength)
            : base(pathPart)
        {
            this.FileChecksum = new byte[16];
            this.customLength = entryLength;
            this.useAutocalculatedValues = false;
        }

        /// <summary>
        /// Creates a new EdataDictionaryFileEntry, with custom file netry length value.
        /// </summary>
        /// <param name="pathPart"></param>
        /// <param name="entryLength"></param>
        /// <param name="fileOffset"></param>
        /// <param name="fileLength"></param>
        /// <param name="fileChecksum"></param>
        public EdataDictionaryFileEntry(
            String pathPart,
            int entryLength,
            long fileOffset,
            long fileLength,
            byte[] fileChecksum)
            : base(pathPart)
        {

            this.FileOffset = fileOffset;
            this.FileLength = fileLength;
            this.FileChecksum = fileChecksum;
            this.customLength = entryLength;
            this.useAutocalculatedValues = false;
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

        public override bool IsEndingEntry()
        {
            if (useAutocalculatedValues)
            {
                return base.IsEndingEntry();
            }
            else 
            {
                return (customLength == 0);
            }
        }

        public override byte[] ToBytes()
        {
            using (var ms = new MemoryStream())
            {
                byte[] buffer = new byte[4];
                ms.Write(buffer, 0, buffer.Length);

                int length = IsEndingEntry() ? 0 : Length;
                buffer = BitConverter.GetBytes(length);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(FileOffset);
                ms.Write(buffer, 0, buffer.Length);

                buffer = BitConverter.GetBytes(FileLength);
                ms.Write(buffer, 0, buffer.Length);

                buffer = FileChecksum;
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

        protected override int GetLengthInBytes()
        {
            if (useAutocalculatedValues)
            {
                int totalLength = 0;

                //FileHeader
                totalLength += 4;

                //Length
                totalLength += sizeof(uint);

                //FileOffset
                totalLength += sizeof(long);

                //FileLength
                totalLength += sizeof(long);

                //FileChecksum
                totalLength += FileChecksum.Length;

                //SubPath
                totalLength += PathPart.Length;

                //zero ended string
                totalLength += 1;

                //supplement to even length.
                totalLength += totalLength % 2 == 0 ? 0 : 1;

                return totalLength;
            }
            else
            {
                return customLength;
            }
        }

    }
}
