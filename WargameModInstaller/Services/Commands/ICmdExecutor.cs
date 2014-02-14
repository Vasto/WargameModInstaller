using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Model.Commands;

namespace WargameModInstaller.Services.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICmdExecutor
    {
        void Execute(CmdExecutionContext context);
        void Execute(CmdExecutionContext context, CancellationToken token);
    }
}
