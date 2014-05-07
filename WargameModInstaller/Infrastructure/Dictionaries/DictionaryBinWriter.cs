using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Dictionaries;

namespace WargameModInstaller.Infrastructure.Dictionaries
{
    /// <summary>
    /// Represents a writer which can writea a TRAD dictionary file to a raw byte data form.
    /// </summary>
    public class DictionaryBinWriter : IDictionaryBinWriter
    {
        public byte[] Write(IEnumerable<DictionaryEntry> entries)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                WriteHeader(stream, (uint)entries.Count());
                WriteEntries(stream, entries);

                return stream.ToArray();
            }
        }

        /// <remarks>
        /// Credits go to enohka for this code.
        /// See more at: https://github.com/enohka/moddingSuite/blob/master/moddingSuite/BL/TradManager.cs
        /// </remarks>
        protected virtual void WriteHeader(MemoryStream ms, uint entriesCount)
        {
            byte[] buffer = Encoding.ASCII.GetBytes("TRAD");
            ms.Write(buffer, 0, buffer.Length);

            buffer = BitConverter.GetBytes(entriesCount);
            ms.Write(buffer, 0, buffer.Length);
        }

        /// <remarks>
        /// Credits go to enohka for this code.
        /// See more at: https://github.com/enohka/moddingSuite/blob/master/moddingSuite/BL/TradManager.cs
        /// </remarks>
        protected virtual void WriteEntries(MemoryStream ms, IEnumerable<DictionaryEntry> entires)
        {
            foreach (var entry in entires)
            {
                entry.OffsetDictionary = (uint)ms.Position;

                // Hash
                ms.Write(entry.Hash, 0, entry.Hash.Length);

                // Content offset : we dont know it yet
                ms.Seek(4, SeekOrigin.Current);

                // Content length
                byte[] buffer = BitConverter.GetBytes(entry.Content.Length);
                ms.Write(buffer, 0, buffer.Length);
            }

            foreach (var entry in entires)
            {
                entry.OffsetContent = (uint)ms.Position;
                byte[] buffer = Encoding.Unicode.GetBytes(entry.Content);
                ms.Write(buffer, 0, buffer.Length);
            }

            foreach (var entry in entires)
            {
                ms.Seek(entry.OffsetDictionary, SeekOrigin.Begin);

                ms.Seek(8, SeekOrigin.Current);

                byte[] buffer = BitConverter.GetBytes(entry.OffsetContent);

                ms.Write(buffer, 0, buffer.Length);
            }
        }

    }
}
