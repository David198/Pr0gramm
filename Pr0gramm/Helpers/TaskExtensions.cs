﻿using System.Threading.Tasks;

namespace Pr0gramm.Helpers
{
    public static class TaskExtensions
    {
        public static void FireAndForget(this Task task)
        {
            // This method allows you to call an async method with awaiting it.
            // Use it when you don't want or need to wait for the task to complete.
        }
    }
}
