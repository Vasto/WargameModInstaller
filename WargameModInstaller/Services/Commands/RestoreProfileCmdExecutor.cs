using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Utilities;
using WargameModInstaller.Model.Commands;
using WargameModInstaller.Services.Commands.Base;
using WargameModInstaller.Services.Install;

namespace WargameModInstaller.Services.Commands
{
    public class RestoreProfileCmdExecutor : CmdExecutorBase<RestoreProfileCmd>
    {
        public RestoreProfileCmdExecutor(RestoreProfileCmd command, IWargameProfileLocator profileLocator)
            : base(command)
        {
            this.ProfileLocator = profileLocator;
            this.TotalSteps = 1;
        }

        protected IWargameProfileLocator ProfileLocator
        {
            get;
            set;
        }

        protected override void ExecuteInternal(CmdExecutionContext context, CancellationToken token)
        {
            InitializeProgress();

            String sourceFullPath = Path.Combine(context.InstallerTargetDirectory, Command.SourcePath);
            if (!PathUtilities.IsValidPath(sourceFullPath))
            {
                throw new CmdExecutionFailedException("A given sourcePath is not a valid path.", Properties.Resources.RestoreSourceErrorMsg);
            }

            var profileRootDir = ProfileLocator.TryGetProfileRootDirectory();
            if (profileRootDir == null || !Directory.Exists(profileRootDir))
            {
                throw new CmdExecutionFailedException("Invalid Profile Root directory.", Properties.Resources.RestoreTargetErrorMsg);
            }

            //Now Create all of the directories
            foreach (String dirPath in Directory.GetDirectories(sourceFullPath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourceFullPath, profileRootDir));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (String filePath in Directory.GetFiles(sourceFullPath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(filePath, filePath.Replace(sourceFullPath, profileRootDir), true);
            }

            SetMaxProgress();
        }
    }
}
