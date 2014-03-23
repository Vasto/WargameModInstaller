using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Model.Commands;

namespace WargameModInstaller.Services.Commands
{
    //To do: Make this class more friendly as a base class for eventual future executors, especially in context of progress notification

    public abstract class CmdGroupExecutorBase<T> : ICmdExecutor, IProgressProvider where T : ICmdGroup
    {
        private readonly ICmdExecutorFactory executorsFactory;

        public CmdGroupExecutorBase(T cmdGroup, ICmdExecutorFactory executorsFactory)
        {
            this.CommandGroup = cmdGroup;
            this.executorsFactory = executorsFactory;
            this.CommandExecutors = CreateExecutorsForGroupCmds();

            this.InitializeProgressProviding();
        }

        public T CommandGroup
        {
            get;
            private set;
        }

        protected IEnumerable<ICmdExecutor> CommandExecutors
        {
            get;
            private set;
        }

        public virtual void Execute(CmdExecutionContext context)
        {
            ExecuteInternal(context);
        }

        public virtual void Execute(CmdExecutionContext context, CancellationToken token)
        {
            ExecuteInternal(context, token);
        }

        protected abstract void ExecuteInternal(CmdExecutionContext context, CancellationToken? token = null);

        private IEnumerable<ICmdExecutor> CreateExecutorsForGroupCmds()
        {
            var executorsList = new List<ICmdExecutor>();
            foreach (var cmd in CommandGroup.Commands)
            {
                var executor = executorsFactory.CreateForCommand(cmd);

                executorsList.Add(executor);
            }

            return executorsList;
        }

        private void InitializeProgressProviding()
        {
            foreach (var executor in CommandExecutors)
            {
                var progressProvider = executor as IProgressProvider;
                if (progressProvider != null)
                {
                    TotalSteps += progressProvider.TotalSteps;
                    CurrentStep += progressProvider.CurrentStep;

                    progressProvider.TotalStepsChanged += SubProgressProviderTotalStepsChangedHandler;
                    progressProvider.CurrentStepChanged += SubProgressProviderCurrentStepChangedHandler;
                    progressProvider.CurrentMessageChanged += SubProgressProviderCurrentMessageChangedHandler;
                }
            }
        }

        private void SubProgressProviderTotalStepsChangedHandler(object sender, StepChangedEventArgs e)
        {
            var stepsDelta = e.NewValue - e.OldValue;
            TotalSteps = TotalSteps + stepsDelta >= 0 ? TotalSteps + stepsDelta : 0;
        }

        private void SubProgressProviderCurrentStepChangedHandler(object sender, StepChangedEventArgs e)
        {
            var stepsDelta = e.NewValue - e.OldValue;
            CurrentStep = CurrentStep + stepsDelta >= 0 ? CurrentStep + stepsDelta : 0;
        }

        private void SubProgressProviderCurrentMessageChangedHandler(object sender, MessageChangedEventArgs e)
        {
            CurrentMessage = e.NewMessage;
        }

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
