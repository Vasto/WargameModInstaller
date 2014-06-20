using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Model.Commands;

namespace WargameModInstaller.Services.Commands.Base
{
    //To do: Make this class more friendly as a base class for eventual future executors...

    public abstract class CmdExecutorBase<T> : ICmdExecutor, IProgressProvider 
        where T : IInstallCmd
    {
        public CmdExecutorBase(T command)
        {
            this.Command = command;
        }

        /// <summary>
        /// Gets a command which has to be executed.
        /// </summary>
        public T Command
        {
            get;
            private set;
        }

        /// <summary>
        /// Executes a wrapped command.
        /// </summary>
        /// <param name="context">Context of execution.</param>
        public virtual void Execute(CmdExecutionContext context)
        {
            Execute(context, CancellationToken.None);
        }

        /// <summary>
        /// Executes a wrapped command.
        /// </summary>
        /// <param name="context">Context of execution.</param>
        /// <param name="token"></param>
        public virtual void Execute(CmdExecutionContext context, CancellationToken token)
        {
            try
            {
                ExecuteInternal(context, token);
            }
            catch (Exception ex)
            {
                if (Command.IsCritical)
                {
                    throw;
                }
                
                WargameModInstaller.Common.Logging.LoggerFactory.Create(this.GetType()).Error(ex);
            }
        }

        protected abstract void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null);

        #region IProgressProvider

        private int totalSteps;
        private int currentStep;
        private String currentMessage;

        public event EventHandler<StepChangedEventArgs> CurrentStepChanged = delegate { };
        public event EventHandler<StepChangedEventArgs> TotalStepsChanged = delegate { };
        public event EventHandler<MessageChangedEventArgs> CurrentMessageChanged = delegate { };

        public int TotalSteps
        {
            get
            {
                return totalSteps;
            }
            set
            {
                var oldValue = totalSteps;
                if (oldValue != value)
                {
                    totalSteps = value;
                    OnTotalStepsChanged(totalSteps, oldValue);
                }
            }
        }

        public int CurrentStep
        {
            get
            {
                return currentStep;
            }
            set
            {
                var oldValue = currentStep;
                if (oldValue != value)
                {
                    currentStep = value;
                    OnCurrentStepChanged(currentStep, oldValue);
                }
            }
        }

        public String CurrentMessage
        {
            get
            {
                return currentMessage;
            }
            set
            {
                var oldValue = currentMessage;
                if (currentMessage != value)
                {
                    currentMessage = value;
                    OnCurrentMessageChanged(currentMessage, oldValue);
                }
            }
        }

        protected void OnTotalStepsChanged(int newValue, int oldValue)
        {
            TotalStepsChanged(this, new StepChangedEventArgs(newValue, oldValue));
        }

        protected void OnCurrentStepChanged(int newValue, int oldValue)
        {
            CurrentStepChanged(this, new StepChangedEventArgs(newValue, oldValue));
        }

        protected void OnCurrentMessageChanged(String newValue, String oldValue)
        {
            CurrentMessageChanged(this, new MessageChangedEventArgs(newValue, oldValue));
        }

        #endregion //IProgressProvider
    }

}
