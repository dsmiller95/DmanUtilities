using System;
using Unity.Jobs;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;

namespace Dman.Utilities
{
    /// <summary>
    /// Utilities to help convert non-IDisposable things into IDisposable things, primarily for use in using statments
    /// These are especially useful in combination with async-await to handle proper cleanup in cancellation or other error states
    /// </summary>
    public static class DisposableAbuse
    {
        public class LambdaDispose : IDisposable
        {
            private Action onDispose;
            public LambdaDispose(Action onDispose)
            {
                this.onDispose = onDispose;
            }

            public void Dispose()
            {
                onDispose();
            }
        }

        /// <summary>
        /// returns a disposable which will release <paramref name="temporaryTexture"/> when disposed
        /// </summary>
        /// <param name="temporaryTexture">a render texture which has been allocated using <see cref="RenderTexture.GetTemporary(int, int)"/></param>
        /// <returns>a disposable handle which will dispose the temporary texture</returns>
        public static IDisposable DisposeTemporaryTexture(this RenderTexture temporaryTexture)
        {
            return new LambdaDispose(() =>
            {
                RenderTexture.ReleaseTemporary(temporaryTexture);
            });
        }

        /// <summary>
        /// returns a disposable which will destroy all objects in the given list
        /// </summary>
        /// <param name="destroyables"></param>
        /// <returns></returns>
        public static IDisposable DisposeDestroyCollection(this IEnumerable<UnityEngine.Object> destroyables)
        {
            if (destroyables == null)
                return null;
            return new LambdaDispose(() =>
            {
                foreach (var destroyable in destroyables)
                {
                    if(destroyable != null)
                    {
                        UnityEngine.Object.Destroy(destroyable);
                    }
                }
            });
        }
    }
}
