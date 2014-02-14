using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WargameModInstaller.Model.Commands
{
    /// <summary>
    /// Basic group holding unrelated commands with the same priority.
    /// </summary>
    public class BasicCmdGroup : ICmdGroup
    {
        private readonly List<IInstallCmd> commands;

        public BasicCmdGroup(IEnumerable<IInstallCmd> commands)
        {
            this.commands = new List<IInstallCmd> (commands);
        }

        public BasicCmdGroup(IEnumerable<IInstallCmd> commands, int priority)
        {
            this.commands = new List<IInstallCmd>(commands);
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

    }

}
