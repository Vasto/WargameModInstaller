using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Extensions;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Model.Containers;

namespace WargameModInstaller.Services.Commands.Base
{
    public abstract class ModNestedTargetBySourceCmdExecutor<T> : CmdExecutorBase<T>
        where T : IInstallCmd, IHasTarget, IHasNestedTarget, IHasSource
    {
        public ModNestedTargetBySourceCmdExecutor(T command)
            : base(command)
        {
            this.TotalSteps = 1;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken token)
        {
            InitializeProgress();

            //Cancel if requested;
            token.ThrowIfCancellationRequested();

            String sourceFullPath = Command.SourcePath.GetAbsoluteOrPrependIfRelative(context.InstallerSourceDirectory);
            if (!File.Exists(sourceFullPath))
            {
                throw new CmdExecutionFailedException(
                    String.Format("Command's source file \"{0}\" doesn't exist.", Command.SourcePath),
                    DefaultExecutionErrorMsg);
            }

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

            //var containerFileReader = new EdataFileReader();
            //var containerFile = CanGetTargetContainerFromContext(context) ?
            //    GetTargetContainerFromContext(context) :
            //    containerFileReader.Read(targetfullPath, false);

            var containerFile = GetTargetContainerFromContext(context);
            var data = new CmdsExecutionData
            {
                ContainerFile = containerFile,
                //ContainerPath = targetfullPath,
                ContentPath = contentPath,
                ModificationSourcePath = sourceFullPath,
            };

            ExecuteCommandsLogic(data);

            //if (!CanGetTargetContainerFromContext(context))
            //{
            //    SaveTargetContainerFile(containerFile, token);
            //}

            SetMaxProgress();
        }

        protected abstract void ExecuteCommandsLogic(CmdsExecutionData data);

        /// <summary>
        /// Represents a set of command's execution data 
        /// validated and modified with the knowledge of current execuion context.
        /// </summary>
        protected class CmdsExecutionData
        {
            public IContainerFile ContainerFile { get; set; }
            //public String ContainerPath { get; set; }
            public String ContentPath { get; set; }
            public String ModificationSourcePath { get; set; }
        }

        protected IContainerFile GetTargetContainerFromContext(CmdExecutionContext context)
        {
            var sharedContainerContext = context as SharedContainerCmdExecContext;
            if (sharedContainerContext == null || 
                sharedContainerContext.ContainerFile == null)
            {
                throw new InvalidOperationException("Cannot obtain a container file from the given execution context");
            }

            return sharedContainerContext.ContainerFile;
        }

    }
}
