using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WargameModInstaller.Common.Entities;
using WargameModInstaller.Common.Utilities;

namespace WargameModInstaller.Services.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public class PercentageProgressManager : IProgressManager
    {
        private readonly HashSet<IProgressProvider> providers;

        private int currentStep;
        private int totalSteps;
        private String currentMessage;
        private int currentProgress;

        /// <summary>
        /// 
        /// </summary>
        public PercentageProgressManager()
        {
            this.providers = new HashSet<IProgressProvider>();
        }

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<ProgressEventArgs> ProgressChanged = delegate { };

        /// <summary>
        /// 
        /// </summary>
        public String CurrentMessage
        {
            get
            {
                return currentMessage;
            }
            set
            {
                var oldValue = currentMessage;
                if (oldValue != value)
                {
                    currentMessage = value;
                    NotifyPorgressChanged(currentProgress, currentMessage);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int CurrentProgress
        {
            get
            {
                return currentProgress;
            }
            set
            {
                var oldValue = currentProgress;
                if (oldValue != value)
                {
                    currentProgress = value;
                    NotifyPorgressChanged(currentProgress, currentMessage);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int MaxProgress
        {
            get { return 100; }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetProgressMin()
        {
            CurrentProgress = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetProgressMax()
        {
            CurrentProgress = MaxProgress;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        public void Register(IProgressProvider provider)
        {
            if(!providers.Contains(provider))
            {   
                providers.Add(provider);
                provider.CurrentStepChanged += ProviderCurrentStepChangedHandler;
                provider.TotalStepsChanged += ProviderTotalStepsChangedHandler;
                provider.CurrentMessageChanged += ProviderCurrentMsgChangedHandler;

                totalSteps += provider.TotalSteps;
                currentStep += provider.CurrentStep;

                CurrentProgress = CalculateCurrentProgress();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        public void Unregister(IProgressProvider provider)
        {
            if (providers.Contains(provider))
            {
                providers.Remove(provider);
                provider.CurrentStepChanged -= ProviderCurrentStepChangedHandler;
                provider.TotalStepsChanged -= ProviderTotalStepsChangedHandler;
                provider.CurrentMessageChanged -= ProviderCurrentMsgChangedHandler;

                totalSteps = (totalSteps - provider.TotalSteps >= 0) ? (totalSteps - provider.TotalSteps) : 0;
                currentStep = (currentStep - provider.CurrentStep >= 0) ? (currentStep - provider.CurrentStep) : 0;

                CurrentProgress = CalculateCurrentProgress();
            }
        }

        /// <summary>
        /// Unregisters all registred progress providers and restes all progress info.
        /// </summary>
        public void Clear()
        {
            foreach (var provider in providers)
            {
                provider.CurrentStepChanged -= ProviderCurrentStepChangedHandler;
                provider.TotalStepsChanged -= ProviderTotalStepsChangedHandler;
                provider.CurrentMessageChanged -= ProviderCurrentMsgChangedHandler;
            }
            providers.Clear();

            totalSteps = 0;
            currentStep = 0;

            SetProgressMin();
        }

        private void ProviderCurrentStepChangedHandler(object sender, StepChangedEventArgs e)
        {
            var valueChange = e.NewValue - e.OldValue;
            currentStep += valueChange;

            CurrentProgress = CalculateCurrentProgress();

            NotifyPorgressChanged(CurrentProgress, CurrentMessage);
        }

        private void ProviderTotalStepsChangedHandler(object sender, StepChangedEventArgs e)
        {
            var valueChange = e.NewValue - e.OldValue;
            totalSteps += valueChange;

            CurrentProgress = CalculateCurrentProgress();

            NotifyPorgressChanged(CurrentProgress, CurrentMessage);
        }

        private void ProviderCurrentMsgChangedHandler(object sender, MessageChangedEventArgs e)
        {
            CurrentMessage = e.NewMessage;

            NotifyPorgressChanged(CurrentProgress, CurrentMessage);
        }

        private void NotifyPorgressChanged(int value, String message = "")
        {
            var args = new ProgressEventArgs(value, message);

            ProgressChanged(this, args);
        }

        private int CalculateCurrentProgress()
        {
            int progress = (int)(((double)currentStep / (double)totalSteps) * 100);
            progress = MathUtilities.Clamp<int>(progress, 0, 100);

            return progress;
        }


    }
}
