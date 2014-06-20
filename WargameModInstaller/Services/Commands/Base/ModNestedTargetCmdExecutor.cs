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

namespace WargameModInstaller.Services.Commands.Base
{
    public abstract class ModNestedTargetCmdExecutor<T> : CmdExecutorBase<T>
        where T : IInstallCmd, IHasTarget, IHasNestedTarget
    {
        public ModNestedTargetCmdExecutor(T command)
            : base(command)
        {
            this.TotalSteps = 1;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken token)
        {
            InitializeProgress();

            //Cancel if requested;
            token.ThrowIfCancellationRequested();

            String targetfullPath = Command.TargetPath.GetAbsoluteOrPrependIfRelative(context.InstallerTargetDirectory);
            if (!File.Exists(targetfullPath))
            {
                throw new CmdExecutionFailedException(
                    String.Format("Command's target file \"{0}\" doesn't exist.", Command.TargetPath),
                    DefaultExecutionErrorMsg);
            }

            String contentPath = Command.NestedTargetPath.LastPart;
            if (contentPath == null)
            {
                throw new CmdExecutionFailedException(
                    "Invalid command's TargetContentPath value.",
                    DefaultExecutionErrorMsg);
            }

            var containerFileReader = new EdataFileReader();
            var containerFile = CanGetTargetContainerFromContext(context) ?
                GetTargetContainerFromContext(context) :
                containerFileReader.Read(targetfullPath, false);

            var data = new CmdsExecutionData
            {
                ContainerFile = containerFile,
                ContainerPath = targetfullPath,
                ContentPath = contentPath,
            };

            ExecuteCommandsLogic(data);

            if (!CanGetTargetContainerFromContext(context))
            {
                SaveTargetContainerFile(containerFile, token);
            }

            SetMaxProgress();
        }

        protected abstract void ExecuteCommandsLogic(CmdsExecutionData data);

        /// <summary>
        /// Represents a set of command's execution data 
        /// validated and modified with the knowledge of current execuion context.
        /// </summary>
        protected class CmdsExecutionData
        {
            public EdataFile ContainerFile { get; set; }
            public String ContainerPath { get; set; }
            public String ContentPath { get; set;}
        }

        //To zamienić na abstrakcje pakietu w przyszłości
        protected virtual bool CanGetTargetContainerFromContext(CmdExecutionContext context)
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

        //To zamienić na abstrakcje pakietu w przyszłości
        protected virtual EdataFile GetTargetContainerFromContext(CmdExecutionContext context)
        {
            var sharedEdataContext = context as SharedEdataCmdExecutionContext;
            if (sharedEdataContext == null)
            {
                throw new InvalidOperationException("Cannot obtain the container file from the given execution context");
            }

            return sharedEdataContext.EdataFile;
        }

        //To zamienić na abstrakcje pakietu w przyszłości
        protected virtual void SaveTargetContainerFile(EdataFile edataFile, CancellationToken token)
        {
            IEdataFileWriter edataWriter = new EdataFileWriter();
            edataWriter.Write(edataFile, token);
        }

    }
}
