using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AsyncFnOnceCell
{
    private bool isRunning;
    private CancellationTokenSource cancellationTokenSource;
    private GameObject ownerGameObject;

    public AsyncFnOnceCell(GameObject cancellationOnDestroy)
    {
        isRunning = false;
        ownerGameObject = cancellationOnDestroy;
    }

    public void ForceRunning()
    {
        this.isRunning = true;
    }
    
    public void CancelThenTryRunNextFrame(Func<CancellationToken, UniTask> asyncFn, string warnIfCannotRun = null)
    {
        TryRunOnNextFrame(asyncFn, warnIfCannotRun).Forget();
    }

    private async UniTask TryRunOnNextFrame(Func<CancellationToken, UniTask> asyncFn, string warnIfCannotRun = null)
    {
        var tries = 10;
        while(isRunning && tries-- > 0){
            TryCancelInternal();
            await UniTask.NextFrame();
            if (isRunning)
            {
                Debug.LogWarning($"#AsyncFnOnceCell: async function failed to cancel. {tries} remaining");
            }
        }
        TryRun(asyncFn, warnIfCannotRun);
    }
    
    /// <summary>
    /// Run the async function if it is not already running. returns true if started, false if already running
    /// </summary>
    /// <param name="asyncFn"></param>
    /// <param name="warnIfCannotRun"></param>
    /// <returns></returns>
    public bool TryRun(Func<CancellationToken, UniTask> asyncFn, string warnIfCannotRun = null)
    {
        if (isRunning)
        {
            if (warnIfCannotRun != null)
            {
                Debug.LogWarning("AsyncFnOnceCell: Cannot run async function because it is already running. " + warnIfCannotRun);
            }
            return false;
        }
        
        cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
            ownerGameObject.GetCancellationTokenOnDestroy());
        var cancelToken = cancellationTokenSource.Token;

        async UniTask RunInternal()
        {
            try
            {
                isRunning = true;
                await asyncFn.Invoke(cancelToken);
            }
            finally
            {
                isRunning = false;
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
        }
        
        RunInternal().Forget();
        return true;
    }

    public void TryCancelInternal()
    {
        if (isRunning)
        {
            cancellationTokenSource?.Cancel();
        }
    }
    
    public bool IsRunning => isRunning;
}