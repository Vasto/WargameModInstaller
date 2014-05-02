using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Dictionaries;

namespace WargameModInstaller.Infrastructure.Dictionaries
{
    public class DictionaryBinReader
    {
        public IEnumerable<DictionaryEntry> Read(byte[] rawDictionaryData)
        {
            using (var stream = new MemoryStream(rawDictionaryData))
            {
                uint entriesCount = ReadHeader(stream);
                var entries = ReadEntries(stream, entriesCount);
                LoadEntriesContent(stream, entries);

                return entries;
            }
        }

        /// <remarks>
        /// Credits go to enohka for this code.
        /// See more at: https://github.com/enohka/moddingSuite/blob/master/moddingSuite/BL/TradManager.cs
        /// </remarks>
        protected virtual uint ReadHeader(MemoryStream ms)
        {
            var buffer = new byte[4];

            ms.Read(buffer, 0, buffer.Length);

            if (Encoding.ASCII.GetString(buffer) != "TRAD")
            {
                throw new ArgumentException("No valid Eugen Systems TRAD (*.dic) file.");
            }

            ms.Read(buffer, 0, buffer.Length);

            return BitConverter.ToUInt32(buffer, 0);
        }

        /// <remarks>
        /// Credits go to enohka for this code.
        /// See more at: https://github.com/enohka/moddingSuite/blob/master/moddingSuite/BL/TradManager.cs
        /// </remarks>
        protected virtual IEnumerable<DictionaryEntry> ReadEntries(MemoryStream ms, uint entriesCount)
        {
            var entries = new List<DictionaryEntry>();

            var buffer = new byte[4];

            for (int i = 0; i < entriesCount; i++)
            {
                var entry = new DictionaryEntry();
                entry.OffsetDictionary = (uint)ms.Position;

                var hashBuffer = new byte[8];

                ms.Read(hashBuffer, 0, hashBuffer.Length);
                entry.Hash = hashBuffer;

                ms.Read(buffer, 0, buffer.Length);
                entry.OffsetContent = BitConverter.ToUInt32(buffer, 0);

                ms.Read(buffer, 0, buffer.Length);
                entry.ContentLength = BitConverter.ToUInt32(buffer, 0);

                entries.Add(entry);
            }

            return entries;
        }

        /// <remarks>
        /// Credits go to enohka for this code.
        /// See more at: https://github.com/enohka/moddingSuite/blob/master/moddingSuite/BL/TradManager.cs
        /// </remarks>
        protected virtual void LoadEntriesContent(MemoryStream ms, IEnumerable<DictionaryEntry> entries)
        {
            foreach (var entry in entries)
            {
                ms.Seek(entry.OffsetContent, SeekOrigin.Begin);

                var buffer = new byte[entry.ContentLength * 2];

                ms.Read(buffer, 0, buffer.Length);

                entry.Content = Encoding.Unicode.GetString(buffer);
            }
        }

    }
}
