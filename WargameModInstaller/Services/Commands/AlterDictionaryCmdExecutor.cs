using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Infrastructure.Dictionaries;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Dictionaries;
using WargameModInstaller.Services.Commands.Base;

namespace WargameModInstaller.Services.Commands
{
    //To do: Ponieważ, zdażało się że stare pliki słownikowe zostały usuwane przez nowe patche, wypadało by to
    // uodpornić na takie sytuacje. Obecnie, gdy taka sytuacja ma miejsce, instalator sie wysypuje ponieważ nie 
    // może znaleźć danego pliku, bo został usunięty.

    public class AlterDictionaryCmdExecutor : ModNestedTargetCmdExecutor<AlterDictionaryCmd>
    {
        public AlterDictionaryCmdExecutor(AlterDictionaryCmd command)
            : base(command)
        {
            this.DefaultExecutionErrorMsg = Properties.Resources.AlterDictionartErrorMsg;
        }

        protected override void ExecuteCommandsLogic(CmdsExecutionData data)
        {
            var contentFile = data.ContainerFile.GetContentFileByPath(data.ContentPath);

            var entries = (new TradDictBinReader()).Read(contentFile.Content);
            var hashToEntriesMap = entries.ToDictionary(key => key.Hash, new ByteArrayComparer());

            RemoveEntries(hashToEntriesMap);

            AddEntries(hashToEntriesMap);

            RenameEntries(hashToEntriesMap);

            var rawDictionaryData = (new TradDictBinWriter()).Write(hashToEntriesMap.Values);
            contentFile.LoadCustomContent(rawDictionaryData);
        }

        private void AddEntries(Dictionary<byte[], TradDictEntry> dictionary)
        {
            foreach (var alteredEntry in Command.AddedEntries)
            {
                byte[] hash = null;
                if (alteredEntry.Key != null)
                {
                    hash = MiscUtilities.HexByteStringToByteArray(alteredEntry.Key);
                }
                else
                {
                    do
                    {
                        hash = GenerateEntryHash();
                    }
                    while (dictionary.ContainsKey(hash));
                }

                //Alter if exists otherwise add a new one.
                TradDictEntry entry;
                if (dictionary.TryGetValue(hash, out entry))
                {
                    entry.Content = alteredEntry.Value;
                }
                else
                {
                    entry = new TradDictEntry();
                    entry.Content = alteredEntry.Value;
                    entry.ContentLength = (uint)alteredEntry.Value.Length;
                    entry.Hash = hash;

                    dictionary.Add(hash, entry);
                }
            }
        }

        private void RemoveEntries(Dictionary<byte[], TradDictEntry> dictionary)
        {
            foreach (var entryHash in Command.RemovedEntries)
            {
                var hash = MiscUtilities.HexByteStringToByteArray(entryHash);

                if (!dictionary.Remove(hash))
                {
                    var warning = String.Format("RemoveEntry operation failed " + 
                        "because entry with the given hash \"{0}\" wasn't found.", entryHash);
                    Common.Logging.LoggerFactory.Create(this.GetType()).Warn(warning);
                }
            }
        }

        private void RenameEntries(Dictionary<byte[], TradDictEntry> dictionary)
        {           
            foreach (var entryToRename in Command.RenamedEntries)
            {
                var hash = MiscUtilities.HexByteStringToByteArray(entryToRename.Key);

                TradDictEntry entry;
                if (dictionary.TryGetValue(hash, out entry))
                {
                    entry.Content = entryToRename.Value;
                }
                else
                {
                    var warning = String.Format("RenameEntry operation failed because " + 
                        "entry with the given hash \"{0}\" wasn't found.", entryToRename.Key);
                    Common.Logging.LoggerFactory.Create(this.GetType()).Warn(warning);
                }
            }
        }

        private byte[] GenerateEntryHash()
        {
            return MiscUtilities.CreateLocalisationHash(MiscUtilities.GenerateCoupon(8, new Random()));
        }

    }
}
