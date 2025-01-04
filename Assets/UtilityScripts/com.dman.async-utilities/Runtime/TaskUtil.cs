using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using MyUtilities;
using UnityEngine;

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
            if (source != null)
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
        
        /// <summary>
        /// Wait on any one task to complete or be cancelled, and then cancel all other tasks
        /// </summary>
        /// <param name="cancel"></param>
        /// <param name="taskGenerators"></param>
        /// <returns></returns>
        public static async UniTask<(int winArgumentIndex, T result)> WhenAnyCancelAll<T>(CancellationToken cancel, params Func<CancellationToken, UniTask<T>>[] taskGenerators)
        {
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancel))
            {
                var tmpToken = cts.Token;
                try
                {
                    return await UniTask.WhenAny(taskGenerators.Select(x => x(tmpToken)));
                }
                finally
                {
                    cts.Cancel();
                }
            }
        }

        public static IDisposable BindCancellationToSelf(this Component destroyable, ref CancellationToken cancel)
        {
            var onDestroyCancel = destroyable.GetCancellationTokenOnDestroy();
            if (cancel == default)
            {
                cancel = onDestroyCancel;
                return null;
            }
            var newSource = CancellationTokenSource.CreateLinkedTokenSource(cancel, onDestroyCancel);
            cancel = newSource.Token;
            return newSource;
        }

        public class SmartDelay
        {
            /// <summary>
            /// an initial estimate of how much time each frame will take
            /// </summary>
            public TimeSpan frameTimeEstimate;
            /// <summary>
            /// how much "time" we have delayed in this frame so far.
            /// This may be negative, if a <see cref="UniTask.Delay"/> call delays
            /// for longer than expected
            /// </summary>
            public TimeSpan totalTimeInFrameSoFar;
            public bool ignoreTimeScale;
            public SmartDelay(bool ignoreTimeScale)
            {
                this.frameTimeEstimate = TimeSpan.FromSeconds(ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime);
                this.totalTimeInFrameSoFar = TimeSpan.Zero;
                this.ignoreTimeScale = ignoreTimeScale;
            }

            public async UniTask PerformDelay(TimeSpan delay, CancellationToken cancel)
            {
                var currentTimeSeconds = ignoreTimeScale ? Time.unscaledTimeAsDouble : Time.timeAsDouble;
                var currentTime = TimeSpan.FromSeconds(currentTimeSeconds);

                // the target time we wish to reach, relative to the time at the beginning of the current frame
                var targetTimeInFrame = totalTimeInFrameSoFar + delay;

                if (targetTimeInFrame <= frameTimeEstimate)
                {
                    totalTimeInFrameSoFar = targetTimeInFrame;
                    return;
                }
                // if our target time in frame is greater than our frame time estimate, we need to invoke Delay to wait through frames
                var startFrame = Time.frameCount;
                await UniTask.Delay(targetTimeInFrame, ignoreTimeScale, cancellationToken: cancel);
                var endFrame = Time.frameCount;

                var nextTimeSeconds = ignoreTimeScale ? Time.unscaledTimeAsDouble : Time.timeAsDouble;
                var nextTime = TimeSpan.FromSeconds(nextTimeSeconds);
                // how much time actually progressed
                var actualTimeDelta = nextTime - currentTime;

                // adjust frame time estimate based on the delay across frames
                this.frameTimeEstimate = actualTimeDelta.Divide(endFrame - startFrame);

                var nextFrameTime = targetTimeInFrame - actualTimeDelta;
                totalTimeInFrameSoFar = nextFrameTime;
            }
        }
    }
}
