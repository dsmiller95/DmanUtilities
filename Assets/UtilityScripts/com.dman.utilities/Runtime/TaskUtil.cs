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
    public static class TaskUtil
    {
        /// <summary>
        /// Await on a job handle to complete, without forcing completion unless cancelled
        /// </summary>
        /// <param name="currentJobHandle"></param>
        /// <param name="cancel"></param>
        /// <returns>true if cancelled</returns>
        public static CancellationToken RefreshToken(ref CancellationTokenSource source)
        {
            if(source != null)
            {
                source.Cancel();
                source.Dispose();
            }
            source = new CancellationTokenSource();

            return source.Token;
        }

        /// <summary>
        /// Wait on any one task to complete or be cancelled, and then cancel all other tasks
        /// </summary>
        /// <param name="cancel"></param>
        /// <param name="taskGenerators"></param>
        /// <returns></returns>
        public static async UniTask<int> WhenAnyCancelAll(CancellationToken cancel, params Func<CancellationToken, UniTask>[] taskGenerators)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancel))
            {
                var tmpToken = cts.Token;
                try
                {
                    var completeIndex = await UniTask.WhenAny(taskGenerators.Select(x => x(tmpToken)));
                    return completeIndex;
                }
                finally
                {
                    cts.Cancel();
                }
            }
        }
    }
}
