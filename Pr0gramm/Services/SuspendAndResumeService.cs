﻿using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Caliburn.Micro;
using Pr0gramm.Activation;
using Pr0gramm.Helpers;

namespace Pr0gramm.Services
{
    internal class SuspendAndResumeService : ActivationHandler<LaunchActivatedEventArgs>
    {
        //// TODO WTS: For more information regarding the application lifecycle and how to handle suspend and resume, please see:
        //// Documentation: https://docs.microsoft.com/windows/uwp/launch-resume/app-lifecycle

        private const string StateFilename = "suspensionState";

        // TODO WTS: This event is fired just before the app enters in background. Subscribe to this event if you want to save your current state.
        public event EventHandler<OnBackgroundEnteringEventArgs> OnBackgroundEntering;

        public async Task SaveStateAsync()
        {
            var suspensionState = new SuspensionState
            {
                SuspensionDate = DateTime.Now
            };

            var target = OnBackgroundEntering?.Target.GetType();
            var onBackgroundEnteringArgs = new OnBackgroundEnteringEventArgs(suspensionState, target);

            OnBackgroundEntering?.Invoke(this, onBackgroundEnteringArgs);

            await ApplicationData.Current.LocalFolder.SaveAsync(StateFilename, onBackgroundEnteringArgs);
        }

        public override async Task HandleInternalAsync(LaunchActivatedEventArgs args)
        {
            await RestoreStateAsync();
        }

        protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
        {
            return args.PreviousExecutionState == ApplicationExecutionState.Terminated;
        }

        private async Task RestoreStateAsync()
        {
            var saveState =
                await ApplicationData.Current.LocalFolder.ReadAsync<OnBackgroundEnteringEventArgs>(StateFilename);
            if (saveState?.Target != null)
            {
                var navigationService = IoC.Get<INavigationService>();
                navigationService.NavigateToViewModel(saveState.Target, saveState.SuspensionState);
            }
        }
    }
}
