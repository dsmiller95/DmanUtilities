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
        /// <param name="throwOnCancelled">when true, will throw an exception if cancelled. otherwise will only return bool indicating cancellation</param>
        /// <param name="maxFameDelay">How many frames to wait. when set to 1, will force completion on frame (n + 2)</param>
        /// <returns>true if cancelled</returns>
        public static async UniTask<bool> ToUniTaskImmediateCompleteOnCancel(
            this JobHandle currentJobHandle, 
            CancellationToken cancel, 
            bool throwOnCancelled = false,
            int maxFameDelay = -1)
        {
            if (TrackPendingJobs)
            {
                PendingAsyncJobs = JobHandle.CombineDependencies(currentJobHandle, PendingAsyncJobs);
            }

            var initialFrame = UnityEngine.Time.frameCount;
            var cancelled = false;
            while (
                !currentJobHandle.IsCompleted && 
                !cancel.IsCancellationRequested && 
                !cancelled &&
                (maxFameDelay < 0 || (initialFrame + maxFameDelay) > UnityEngine.Time.frameCount))
            {
                //var (cancelledTask, registration) = cancel.ToUniTask();
                try
                {
                    var completedIndex = await UniTask.WhenAny(
                        UniTask.Yield(PlayerLoopTiming.PreUpdate, cancel).SuppressCancellationThrow(),
                        UniTask.Yield(PlayerLoopTiming.Update, cancel).SuppressCancellationThrow(),
                        UniTask.Yield(PlayerLoopTiming.PostLateUpdate, cancel).SuppressCancellationThrow()
                        );
                }catch(OperationCanceledException)
                {
                    cancelled = true;
                }
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
