using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Infrastructure.Containers;
using WargameModInstaller.Model.Commands;

namespace WargameModInstaller.Services.Commands.Base
{
    //To do: Make this class more friendly as a base class for eventual future executors, especially in context of progress notification

    public abstract class CmdGroupExecutorBase<T> : ICmdExecutor, IProgressProvider where T : ICmdGroup
    {
        private List<Tuple<IInstallCmd, ICmdExecutor>> cmdExecutorPairs;

        public CmdGroupExecutorBase(
            T cmdGroup, 
            ICmdExecutorFactory executorsFactory)
        {
            this.CommandGroup = cmdGroup;
            this.ExecutorFactory = executorsFactory;
            this.cmdExecutorPairs = CreateCommandExecutorPairs();

            this.InitializeProgressProviding();
        }

        public T CommandGroup
        {
            get;
            private set;
        }

        protected ICmdExecutorFactory ExecutorFactory
        {
            get;
            private set;
        }

        protected IEnumerable<ICmdExecutor> Executors
        {
            get { return cmdExecutorPairs.Select(x => x.Item2); }
        }

        public virtual void Execute(CmdExecutionContext context)
        {
            Execute(context, CancellationToken.None);
        }

        public abstract void Execute(CmdExecutionContext context, CancellationToken token);

        protected IInstallCmd GetExecutorsAssociatedCommand(ICmdExecutor executor)
        {
            return cmdExecutorPairs.Single(x => x.Item2 == executor).Item1;
        }

        private List<Tuple<IInstallCmd, ICmdExecutor>> CreateCommandExecutorPairs()
        {
            var results = new List<Tuple<IInstallCmd, ICmdExecutor>>();
            foreach (var cmd in CommandGroup.Commands)
            {
                var executor = ExecutorFactory.CreateForCommand(cmd);
                results.Add(new Tuple<IInstallCmd, ICmdExecutor>(cmd, executor));
            }

            return results;
        }

        private void InitializeProgressProviding()
        {
            foreach (var executor in Executors)
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
