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
using WargameModInstaller.Services.Commands;

namespace WargameModInstaller.Services.Commands
{
    public class SharedEdataCmdGroupExecutor : CmdGroupExecutorBase<SharedEdataCmdGroup>
    {
        public SharedEdataCmdGroupExecutor(SharedEdataCmdGroup cmdGroup, ICmdExecutorFactory executorsFactory)
            : base(cmdGroup, executorsFactory)
        {
            this.TotalSteps++;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null)
        {
            //Ta metoda bez bloków łapania wyjątków, w przypadku ewentualnego wyjątku pochodzącego z kodu z poza execute, spowoduje 
            //wykrzaczenie się całej instalacji. Może trzeba zaimplementować IsCritical także dla CmdGroup...

            String targetfullPath = CommandGroup.SharedEdataPath.GetAbsoluteOrPrependIfRelative(context.InstallerTargetDirectory);
            if (!File.Exists(targetfullPath))
            {
                //Jeśli ten plik nie istnieje to szlag wszystkie komendy wewnętrzne.
                throw new CmdExecutionFailedException("Specified Edata file doesn't exist",
                    String.Format(WargameModInstaller.Properties.Resources.NotExistingFileOperationErrorParametrizedMsg, targetfullPath));
            }

            CurrentStep = 0;

            //should it be sorrounded with a try-catch?

            EdataReader edataReader = new EdataReader();
            EdataFile edataFile = edataReader.ReadAll(targetfullPath, false);

            var newExecutionContext = new SharedEdataCmdExecutionContext(
                context.InstallerSourceDirectory,
                context.InstallerTargetDirectory,
                edataFile);

            foreach (var executor in CommandExecutors)
            {
                if (token.HasValue)
                {
                    executor.Execute(newExecutionContext, token.Value);
                }
                else
                {
                    executor.Execute(newExecutionContext);
                }
            }

            IEdataWriter edataWriter = new EdataWriter();
            if (token.HasValue)
            {
                edataWriter.Write(edataFile, token.Value);
            }
            else
            {
                edataWriter.Write(edataFile);
            }

            //Set max, completed
            CurrentStep = TotalSteps;
        }

    }

}
