using System;
using Unity.Jobs;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Dman.Utilities
{
    /// <summary>
    /// Utility to help with locking of resources which can be read in parallel by multiple jobs,
    ///     but still need to be edited occasionally
    /// </summary>
    public static class JobHandleExtensions
    {
        public static bool TrackPendingJobs = false;
        public static JobHandle PendingAsyncJobs { get; private set; } = default(JobHandle);

        /// <summary>
        /// Await on a job handle to complete, without forcing completion unless cancelled
        /// </summary>
        /// <param name="currentJobHandle"></param>
        /// <param name="cancel"></param>
        /// <returns>true if cancelled</returns>
        public static async UniTask<bool> ToUniTaskImmediateCompleteOnCancel(this JobHandle currentJobHandle, CancellationToken cancel, bool throwOnCancelled = false)
        {
            if (TrackPendingJobs)
            {
                PendingAsyncJobs = JobHandle.CombineDependencies(currentJobHandle, PendingAsyncJobs);
            }

            var cancelled = false;
            while (!currentJobHandle.IsCompleted && !cancel.IsCancellationRequested && !cancelled)
            {
                var (cancelledTask, registration) = cancel.ToUniTask();
                var completedIndex = await UniTask.WhenAny(
                    cancelledTask,
                    UniTask.Yield(PlayerLoopTiming.PreUpdate, cancel).SuppressCancellationThrow(),
                    UniTask.Yield(PlayerLoopTiming.Update, cancel).SuppressCancellationThrow(),
                    UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancel).SuppressCancellationThrow()
                    );
                cancelled = completedIndex == 0;
                registration.Dispose();
            }
            currentJobHandle.Complete();
            var wasCancelled = cancelled || cancel.IsCancellationRequested;
            if (throwOnCancelled && wasCancelled)
            {
                throw new OperationCanceledException();
            }
            return wasCancelled;
        }
    }
}
