using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Background;
using Pr0gramm.Activation;
using Pr0gramm.BackgroundTasks;
using Pr0gramm.Helpers;

namespace Pr0gramm.Services
{
    internal class BackgroundTaskService : ActivationHandler<BackgroundActivatedEventArgs>
    {
        private static readonly Lazy<IEnumerable<BackgroundTask>> BackgroundTaskInstances =
            new Lazy<IEnumerable<BackgroundTask>>(CreateInstances);

        public static IEnumerable<BackgroundTask> BackgroundTasks => BackgroundTaskInstances.Value;

        public void RegisterBackgroundTasks()
        {
            foreach (var task in BackgroundTasks)
                task.Register();
        }

        public static BackgroundTaskRegistration GetBackgroundTasksRegistration<T>()
            where T : BackgroundTask
        {
            if (!BackgroundTaskRegistration.AllTasks.Any(t => t.Value.Name == typeof(T).Name))
                return null;

            return (BackgroundTaskRegistration) BackgroundTaskRegistration.AllTasks
                .FirstOrDefault(t => t.Value.Name == typeof(T).Name).Value;
        }

        public void Start(IBackgroundTaskInstance taskInstance)
        {
            var task = BackgroundTasks.FirstOrDefault(b => b.Match(taskInstance?.Task?.Name));

            if (task == null)
                return;

            task.RunAsync(taskInstance).FireAndForget();
        }

        public override async Task HandleInternalAsync(BackgroundActivatedEventArgs args)
        {
            Start(args.TaskInstance);

            await Task.CompletedTask;
        }

        private static IEnumerable<BackgroundTask> CreateInstances()
        {
            var backgroundTasks = new List<BackgroundTask>();

            backgroundTasks.Add(new BackgroundTask1());
            return backgroundTasks;
        }
    }
}
