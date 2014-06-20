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
    public class BasicCmdGroupExecutor : CmdGroupExecutorBase<BasicCmdGroup>
    {
        public BasicCmdGroupExecutor(BasicCmdGroup cmdGroup, ICmdExecutorFactory executorsFactory)
            : base(cmdGroup, executorsFactory)
        {

        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken token)
        {
            CurrentStep = 0;

            foreach (var executor in CommandExecutors)
            {
                executor.Execute(context, token);
            }

            CurrentStep = TotalSteps;
        }

    }

}
