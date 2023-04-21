namespace Common.Helpers
{
    public static class AsyncHelper
    {
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            if (SynchronizationContext.Current == null)
            {
                return Task.Run(func).GetAwaiter().GetResult();
            }

            TResult result = default;
            var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var t = new Task(async () => { result = await func(); });
            t.RunSynchronously(taskScheduler);
            t.Wait();

            return result;
        }

        public static void RunSync(Func<Task> func)
        {
            if (SynchronizationContext.Current == null)
            {
                Task.Run(func).GetAwaiter().GetResult();
            }
            else
            {
                var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
                var t = new Task(async () => { await func(); });
                t.RunSynchronously(taskScheduler);
                t.Wait();
            }
        }
    }
}
