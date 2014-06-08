using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Services.Edata;

namespace WargameModInstaller.Services.Commands
{
    //Kroki trochę sie nie zgadzają, bo te dodakowe rebuildy nie są brane pod uwagę, trzeba było by określić możliwą ilosc
    //Rebuildów w konstruktorze...

    public class EdataCmdGroupExecutor : CmdGroupExecutorBase<EdataCmdGroup>
    {
        public EdataCmdGroupExecutor(EdataCmdGroup cmdGroup, ICmdExecutorFactory executorsFactory)
            : base(cmdGroup, executorsFactory)
        {
            this.TotalSteps++;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null)
        {
            //Ta metoda bez bloków łapania wyjątków, w przypadku ewentualnego wyjątku pochodzącego z kodu z poza execute, spowoduje 
            //wykrzaczenie się całej instalacji. Może trzeba zaimplementować IsCritical także dla CmdGroup...

            String targetfullPath = CommandGroup.CommonEdataPath.GetAbsoluteOrPrependIfRelative(context.InstallerTargetDirectory);
            if (!File.Exists(targetfullPath))
            {
                //Jeśli ten plik nie istnieje to szlag wszystkie komendy wewnętrzne.
                throw new CmdExecutionFailedException(String.Format("Specified Edata file: \"{0}\" doesn't exist", targetfullPath),
                    String.Format(Properties.Resources.NotExistingFileOperationErrorParametrizedMsg, targetfullPath));
            }

            CurrentStep = 0;

            //should it be sorrounded with a try-catch?

            EdataFileReader edataFileReader = new EdataFileReader();
            EdataFile edataFile = edataFileReader.Read(targetfullPath, false);

            var newExecutionContext = new SharedEdataCmdExecutionContext(
                context.InstallerSourceDirectory,
                context.InstallerTargetDirectory,
                edataFile);

            var edataContentFiles = GetContentFilesToBeLoaded(edataFile);
            EdataContentManager edataContentManager = new EdataContentManager(edataContentFiles);
            edataContentManager.MaxLoadedContentReached += (sender, args) =>
            {
                SaveEdataChanges(edataFile, token);
                edataContentManager.FreeLoadedContent();
            };

            foreach (var executor in CommandExecutors)
            {
                executor.Execute(newExecutionContext, token.Value);
            }

            SaveEdataChanges(edataFile, token);

            //Set max, completed
            CurrentStep = TotalSteps;
        }

        private void SaveEdataChanges(EdataFile edataFile, CancellationToken? token = null)
        {
            CurrentMessage = String.Format(Properties.Resources.RebuildingParametrizedMsg, this.CommandGroup.CommonEdataPath);

            IEdataFileWriter edataWriter = new EdataFileWriter();
            edataWriter.Write(edataFile, token.Value);
        }

        private IEnumerable<EdataContentFile> GetContentFilesToBeLoaded(EdataFile ownerFile)
        {
            var contentFilesPaths = CommandGroup
                .Commands
                .OfType<IHasTargetContent>()
                .Select(c => c.TargetContentPath);

            var contentFilesToBeLoaded = new List<EdataContentFile>();
            foreach (var path in contentFilesPaths)
            {
                var contentFile = ownerFile.GetContentFileByPath(path);
                contentFilesToBeLoaded.Add(contentFile);
            }

            return contentFilesToBeLoaded;
        }

    }

}
