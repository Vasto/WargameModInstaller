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

namespace WargameModInstaller.Services.Commands
{
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
                throw new CmdExecutionFailedException("Specified Edata file doesn't exist",
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

            foreach (var executor in CommandExecutors)
            {
                executor.Execute(newExecutionContext, token.Value);
            }

            CurrentMessage = String.Format(Properties.Resources.RebuildingParametrizedMsg, this.CommandGroup.CommonEdataPath);

            IEdataFileWriter edataWriter = new EdataFileWriter();
            edataWriter.Write(edataFile, token.Value);

            //Set max, completed
            CurrentStep = TotalSteps;
        }

    }

}
