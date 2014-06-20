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
using WargameModInstaller.Services.Commands.Base;

namespace WargameModInstaller.Services.Commands
{
    //To do: reconsider method names, reconsider class name

    public abstract class EdataTargetCmdExecutorBase<T> : CmdExecutorBase<T>
        where T : IInstallCmd, IHasTarget, IHasNestedTarget
    {
        public EdataTargetCmdExecutorBase(T command)
            : base(command)
        {

        }

        protected bool CanGetEdataFromContext(CmdExecutionContext context)
        {
            var sharedEdataContext = context as SharedEdataCmdExecutionContext;
            if (sharedEdataContext != null)
            {
                return sharedEdataContext.EdataFile != null;
            }
            else
            {
                return false;
            }
        }

        protected EdataFile GetEdataFromContext(CmdExecutionContext context)
        {
            var sharedEdataContext = context as SharedEdataCmdExecutionContext;
            if (sharedEdataContext != null)
            {
                return sharedEdataContext.EdataFile;
            }
            else
            {
                throw new InvalidOperationException("Cannot get an Edata file from the given context");
            }
        }

        protected void SaveEdataFile(EdataFile edataFile, CancellationToken? token = null)
        {
            IEdataFileWriter edataWriter = new EdataFileWriter();
            if (token.HasValue)
            {
                edataWriter.Write(edataFile, token.Value);
            }
            else
            {
                edataWriter.Write(edataFile);
            }
        }

    }

}
