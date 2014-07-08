using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Services.Commands.Base;

namespace WargameModInstaller.Services.Commands
{
    /// <summary>
    /// Póki co robi za obiekt grupujący komendy niezależne od zawartości plików kontentu.
    /// </summary>
    public class BasicCmdsExecutor : CmdGroupExecutorBase<BasicCmdGroup>
    {
        public BasicCmdsExecutor(
            BasicCmdGroup cmdGroup,
            ICmdExecutorFactory executorsFactory)
            : base(cmdGroup, executorsFactory)
        {

        }

        public override void Execute(CmdExecutionContext context, CancellationToken token)
        {
            CurrentStep = 0;

            foreach (var executor in Executors)
            {
                executor.Execute(context, token);
            }

            CurrentStep = TotalSteps;
        }

    }

}
