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
    public class BackupProfileCmdExecutor : CmdExecutorBase<BackupProfileCmd>
    {
        public BackupProfileCmdExecutor(BackupProfileCmd command, IWargameProfileLocator profileLocator)
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

            var profileDirectories = ProfileLocator.GetProfileDirectories();
            if (profileDirectories.Count() == 0)
            {
                throw new CmdExecutionFailedException("No profile directories found.", Properties.Resources.BackupSourceErrorMsg);
            }

            String targetFullPath = Path.Combine(context.InstallerTargetDirectory, Command.TargetPath);
            if (!PathUtilities.IsValidPath(targetFullPath))
            {
                throw new CmdExecutionFailedException("A given targetPath is not a valid path.", Properties.Resources.BackupTargetErrorMsg);
            }

            foreach (var profileDir in profileDirectories)
            {
                var profileRootDir = ProfileLocator.TryGetProfileRootDirectory();
                var profileSubPath = profileDir.Substring(profileRootDir.Length + 1);
                var destinationPath = Path.Combine(targetFullPath, profileSubPath);

                //Now Create all of the directories
                foreach (String dirPath in Directory.GetDirectories(profileDir, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(profileDir, destinationPath));
                }

                //Copy all the files & Replaces any files with the same name
                foreach (String filePath in Directory.GetFiles(profileDir, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(filePath, filePath.Replace(profileDir, destinationPath), true);
                }
            }

            SetMaxProgress();
        }
    }
}
