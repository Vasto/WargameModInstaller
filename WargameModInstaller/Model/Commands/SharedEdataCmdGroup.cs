using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;

namespace WargameModInstaller.Model.Commands
{
    public class SharedEdataCmdGroup : ICmdGroup
    {
        private readonly List<IInstallCmd> commands;

        public SharedEdataCmdGroup(IEnumerable<IInstallCmd> commands, InstallEntityPath edataPath)
        {
            this.commands = new List<IInstallCmd> (commands);
            this.SharedEdataPath = edataPath;
        }

        public SharedEdataCmdGroup(IEnumerable<IInstallCmd> commands, InstallEntityPath edataPath, int priority)
        {
            this.commands = new List<IInstallCmd>(commands);
            this.SharedEdataPath = edataPath;
            this.Priority = priority;
        }

        //Should change priority off all contained commands?
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

        public InstallEntityPath SharedEdataPath
        {
            get;
            private set;
        }

    }

}
