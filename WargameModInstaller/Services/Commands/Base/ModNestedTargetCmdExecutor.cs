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
                    String.Format("One of the commands refers to a non-existing target file: \"{0}\"", Command.TargetPath),
                    DefaultExecutionErrorMsg);
            }

            String contentPath = Command.NestedTargetPath.LastPart;
            if (contentPath == null)
            {
                throw new CmdExecutionFailedException(
                    "One of the commands has an invalid targetContentPath value.",
                    DefaultExecutionErrorMsg);
            }

            //if(!CanGetTargetContainerFromContext(context))
            //{
            //    GetTargetContainerFromContext(context);
            //}

            var containerFile = GetTargetContainerFromContext(context);
            var data = new CmdsExecutionData
            {
                ContainerFile = containerFile,
                //ContainerPath = targetfullPath,
                ContentPath = contentPath,
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
            public String ContentPath { get; set;}
        }

        //To zamienić na abstrakcje pakietu w przyszłości
        //protected virtual bool CanGetTargetContainerFromContext(CmdExecutionContext context)
        //{
        //    var sharedEdataContext = context as SharedContainerCmdExecContext;
        //    if (sharedEdataContext != null)
        //    {
        //        return sharedEdataContext.ContainerFile != null;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        protected IContainerFile GetTargetContainerFromContext(CmdExecutionContext context)
        {
            var sharedContainerContext = context as SharedContainerCmdExecContext;
            if (sharedContainerContext == null ||
                sharedContainerContext.ContainerFile == null)
            {
                throw new InvalidOperationException("Cannot obtain a container file from the given execution context.");
            }

            return sharedContainerContext.ContainerFile;
        }

        //To zamienić na abstrakcje pakietu w przyszłości
        //Update to wywalić, i wogole cała otoczke zapisu odczytu pliku kontenera przenieśc do egzekutora grupy.
        //protected virtual void SaveTargetContainerFile(IContainerFile containerFile, CancellationToken token)
        //{
        //    IContainerWriterService writer = new ContainerWriterService();
        //    writer.WriteFile(containerFile, token);
        //}

    }
}
