﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;

namespace Pr0gramm.BackgroundTasks
{
    public sealed class BackgroundTask1 : BackgroundTask
    {
        private volatile bool _cancelRequested = false;
        private BackgroundTaskDeferral _deferral;
        private IBackgroundTaskInstance _taskInstance;
        public static string Message { get; set; }

        public override void Register()
        {
            var taskName = GetType().Name;

            if (!BackgroundTaskRegistration.AllTasks.Any(t => t.Value.Name == taskName))
            {
                var builder = new BackgroundTaskBuilder
                {
                    Name = taskName
                };

                // TODO WTS: Define your trigger here and set your conditions
                // Note conditions are optional
                // Documentation: https://docs.microsoft.com/windows/uwp/launch-resume/create-and-register-an-inproc-background-task
                builder.SetTrigger(new TimeTrigger(15, false));
                builder.AddCondition(new SystemCondition(SystemConditionType.UserPresent));

                builder.Register();
            }
        }

        public override Task RunAsyncInternal(IBackgroundTaskInstance taskInstance)
        {
            if (taskInstance == null)
                return null;

            _deferral = taskInstance.GetDeferral();

            return Task.Run(() =>
            {
                //// TODO WTS: Insert the code that should be executed in the background task here.
                //// This sample initializes a timer that counts to 100 in steps of 10.  It updates Message each time.

                //// Documentation:
                ////      * General: https://docs.microsoft.com/en-us/windows/uwp/launch-resume/support-your-app-with-background-tasks
                ////      * Debug: https://docs.microsoft.com/en-us/windows/uwp/launch-resume/debug-a-background-task
                ////      * Monitoring: https://docs.microsoft.com/windows/uwp/launch-resume/monitor-background-task-progress-and-completion

                //// To show the background progress and message on any page in the application,
                //// subscribe to the Progress and Completed events.
                //// You can do this via "BackgroundTaskService.GetBackgroundTasksRegistration"

                _taskInstance = taskInstance;
                ThreadPoolTimer.CreatePeriodicTimer(SampleTimerCallback, TimeSpan.FromSeconds(1));
            });
        }

        public override void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // TODO WTS: Insert code to handle the cancelation request here.
            // Documentation: https://docs.microsoft.com/windows/uwp/launch-resume/handle-a-cancelled-background-task
        }

        private void SampleTimerCallback(ThreadPoolTimer timer)
        {
            if (_cancelRequested == false && _taskInstance.Progress < 100)
            {
                _taskInstance.Progress += 10;
                Message = $"Background Task {_taskInstance.Task.Name} running";
            }
            else
            {
                timer.Cancel();

                if (_cancelRequested)
                    Message = $"Background Task {_taskInstance.Task.Name} cancelled";
                else
                    Message = $"Background Task {_taskInstance.Task.Name} finished";

                _deferral?.Complete();
            }
        }
    }
}
