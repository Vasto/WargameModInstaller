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

            foreach (var alteredEntry in Command.AlteredEntries)
            {
                var hash = MiscUtilities.HexByteStringToByteArray(alteredEntry.Key);

                TradDictEntry entry;
                if (hashToEntriesMap.TryGetValue(hash, out entry))
                {
                    entry.Content = alteredEntry.Value;
                }
                else
                {
                    var warning = String.Format("The entry with the given hash \"{0}\" wasn't found.", alteredEntry.Key);
                    Common.Logging.LoggerFactory.Create(this.GetType()).Warn(warning);
                }
            }

            var rawDictionaryData = (new TradDictBinWriter()).Write(entries);
            contentFile.Content = rawDictionaryData;
        }

    }
}
