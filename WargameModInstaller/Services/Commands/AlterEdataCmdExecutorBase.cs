using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Infrastructure.Edata;
using WargameModInstaller.Infrastructure.Image;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Edata;
using WargameModInstaller.Model.Image;

namespace WargameModInstaller.Services.Commands
{
    //To do: reconsider method names, reconsider class name

    public abstract class AlterEdataCmdExecutorBase<T> : CmdExecutorBase<T> 
        where T : IInstallCmd, IHasTarget, IHasTargetContent
    {
        public AlterEdataCmdExecutorBase(T command)
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
