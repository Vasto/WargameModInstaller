using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Infrastructure.Containers;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Services.Commands.Base;
using WargameModInstaller.Services.Containers;

namespace WargameModInstaller.Services.Commands
{
    //Kroki trochę sie nie zgadzają, bo te dodakowe rebuildy nie są brane pod uwagę, trzeba było by określić możliwą ilosc
    //Rebuildów w konstruktorze...

    public class SharedTargetCmdsExecutor : ContainerCmdsExecutorBase<SharedTargetCmdGroup>
    {
        public SharedTargetCmdsExecutor(
            SharedTargetCmdGroup cmdGroup,
            ICmdExecutorFactory executorsFactory,
            IContainerReaderService containerReader,
            IContainerWriterService containerWriter)
            : base(cmdGroup, executorsFactory, containerReader, containerWriter)
        {
            this.TotalSteps++;
        }

        public override void Execute(CmdExecutionContext context, CancellationToken token)
        {
            //Ta metoda bez bloków łapania wyjątków, w przypadku ewentualnego wyjątku pochodzącego z kodu z poza execute, spowoduje 
            //wykrzaczenie się całej instalacji. Może trzeba zaimplementować IsCritical także dla CmdGroup...

            String targetfullPath = CommandGroup.TargetPath
                .GetAbsoluteOrPrependIfRelative(context.InstallerTargetDirectory);
            if (!File.Exists(targetfullPath))
            {
                //Jeśli ten plik nie istnieje to szlag wszystkie komendy wewnętrzne.
                throw new CmdExecutionFailedException(
                    String.Format("A specified target file: \"{0}\" doesn't exist", targetfullPath),
                    String.Format(Properties.Resources.NotExistingFileOperationErrorParamMsg, targetfullPath));
            }

            CurrentStep = 0;

            //should it be sorrounded with a try-catch?

            var containerFile = ContainerReaderService.ReadFile(targetfullPath, false);

            ContainerFileLoadManager loadManager = new ContainerFileLoadManager();
            loadManager.MaxLoadReached += (sender, args) =>
            {
                CurrentMessage = String.Format(Properties.Resources.RebuildingParametrizedMsg, CommandGroup.TargetPath);

                ContainerWriterService.WriteFile(containerFile, token);
                loadManager.FreeManagedFilesLoad();
            };

            var newExecutionContext = new SharedContainerCmdExecContext(
                context.InstallerSourceDirectory,
                context.InstallerTargetDirectory,
                containerFile);

            foreach (var executor in Executors)
            {
                var cmd = GetExecutorsAssociatedCommand(executor);
                var paths = GetPathsOfContentFiles(cmd);
                var contentFiles = GetContentFiles(containerFile, paths);

                loadManager.AddManagedFiles(contentFiles);

                LoadContentFiles(contentFiles);

                executor.Execute(newExecutionContext, token);
            }

            CurrentMessage = String.Format(Properties.Resources.RebuildingParametrizedMsg, CommandGroup.TargetPath);

            ContainerWriterService.WriteFile(containerFile, token);

            //Set max, completed
            CurrentStep = TotalSteps;
        }

    }

}
