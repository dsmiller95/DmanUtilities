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
    }
}
