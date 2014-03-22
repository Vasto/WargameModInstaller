using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    public class MultiLevelEdataCmdGroup : ICmdGroup
    {
        private readonly List<IInstallCmd> commands;

        public MultiLevelEdataCmdGroup(
            IEnumerable<IInstallCmd> commands, 
            InstallEntityPath edataPath,
            ContentPath commonMultiLevelEdataPath)
        {
            this.commands = new List<IInstallCmd> (commands);
            this.CommonEdataPath = edataPath;
            this.CommonMultiLevelEdataPath = commonMultiLevelEdataPath;
        }

        public MultiLevelEdataCmdGroup(
            IEnumerable<IInstallCmd> commands, 
            InstallEntityPath edataPath,
            ContentPath commonMultiLevelEdataPath,
            int priority)
        {
            this.commands = new List<IInstallCmd>(commands);
            this.CommonEdataPath = edataPath;
            this.CommonMultiLevelEdataPath = commonMultiLevelEdataPath;
            this.Priority = priority;
        }

        public int Priority
        {
            get;
            set;
        }

        public IEnumerable<IInstallCmd> Commands
        {
            get
            {
                return commands;
            }
        }

        public int CommandsCount
        {
            get
            {
                return commands.Count;
            }
        }

        public InstallEntityPath CommonEdataPath
        {
            get;
            private set;
        }

        public ContentPath CommonMultiLevelEdataPath
        {
            get;
            private set;
        }

    }
}
