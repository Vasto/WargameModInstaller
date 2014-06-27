using Ninject;
using Ninject.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Model.Commands;

namespace WargameModInstaller.Services.Commands
{
    public class CmdExecutorFactory : ICmdExecutorFactory
    {
        private readonly IKernel kernel;

        private IDictionary<Type, Type> commandToExecutorsMap;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kernel"></param>
        public CmdExecutorFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        protected IDictionary<Type, Type> CommandToExecutorsMap
        {
            get
            {
                if (commandToExecutorsMap == null)
                {
                    commandToExecutorsMap = CreateCommandToExecutorsMap();
                }

                return commandToExecutorsMap;
            }
        }

        public virtual ICmdExecutor CreateForCommandGroup(ICmdGroup commandGroup)
        {
            foreach (var pair in CommandToExecutorsMap)
            {
                if (pair.Key.IsInstanceOfType(commandGroup))
                {
                    var argument = new ConstructorArgument("cmdGroup", commandGroup);
                    var executorInstance = (ICmdExecutor)kernel.Get(pair.Value, argument);

                    return executorInstance;
                }
            }

            throw new InvalidOperationException("Cannot create an executor for the provided command group");
        }

        public virtual ICmdExecutor CreateForCommand(IInstallCmd command) 
        {
            foreach (var pair in CommandToExecutorsMap)
            {
                if (pair.Key.IsInstanceOfType(command))
                {
                    var argument = new ConstructorArgument("command", command);
                    var executorInstance = (ICmdExecutor)kernel.Get(pair.Value, argument);

                    return executorInstance;
                }
            }

            throw new InvalidOperationException("Cannot create an executor for the provided command");
        }

        protected virtual IDictionary<Type, Type> CreateCommandToExecutorsMap()
        {
            var map = new Dictionary<Type, Type>();
            map.Add(typeof(CopyGameFileCmd), typeof(CopyGameFileCmdExecutor));
            map.Add(typeof(CopyModFileCmd), typeof(CopyModFileCmdExecutor));
            map.Add(typeof(RemoveFileCmd), typeof(RemoveCmdExecutor));
            map.Add(typeof(ReplaceImageCmd), typeof(ReplaceImageCmdExecutor));
            map.Add(typeof(ReplaceImageTileCmd), typeof(ReplaceImageTileCmdExecutor));
            map.Add(typeof(ReplaceImagePartCmd), typeof(ReplaceImagePartCmdExecutor));
            map.Add(typeof(ReplaceContentCmd), typeof(ReplaceContentCmdExecutor));
            map.Add(typeof(AlterDictionaryCmd), typeof(AlterDictionaryCmdExecutor));
            map.Add(typeof(AddContentCmd), typeof(AddContentCmdExecutor));
            map.Add(typeof(AddImageCmd), typeof(AddImageCmdExecutor));
            map.Add(typeof(BasicCmdGroup), typeof(BasicCmdsExecutor));
            map.Add(typeof(SharedTargetCmdGroup), typeof(SharedTargetCmdsExecutor));
            map.Add(typeof(SharedNestedTargetCmdGroup), typeof(SharedNestedTargetCmdsExecutor));

            return map;
        }

    }

}
