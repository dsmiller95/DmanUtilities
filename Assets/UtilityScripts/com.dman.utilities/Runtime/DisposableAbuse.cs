using System;
using System.Collections.Generic;
using UnityEngine;

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
            private bool disposed;
            public LambdaDispose(Action onDispose)
            {
                this.onDispose = onDispose;
                disposed = false;
            }

            public void Dispose()
            {
                if (disposed) return;
                onDispose();
                disposed = true;
            }

            public static implicit operator LambdaDispose(UnityEngine.Object destroyable)
            {
                return DestroyOnDisposeInternal(destroyable, useImmediate: false);
            }
        }
        
        public static IDisposable EnableThenDisable(this UnityEngine.GameObject enableable)
        {
            enableable.SetActive(true);
            return new LambdaDispose(() =>
            {
                enableable.SetActive(false);
            });
        }

        public static IDisposable DestroyOnDispose(this UnityEngine.Object destroyable, bool useImmediate = false)
        {
            return DestroyOnDisposeInternal(destroyable, useImmediate);
        }

        private static LambdaDispose DestroyOnDisposeInternal(UnityEngine.Object destroyable, bool useImmediate = false)
        {
            if (destroyable == null) return null;
            return new LambdaDispose(() =>
            {
                if (destroyable != null)
                {
                    if (useImmediate)
                    {
                        UnityEngine.Object.DestroyImmediate(destroyable);
                    }
                    else
                    {
                        UnityEngine.Object.Destroy(destroyable);
                    }
                }
            });
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
                    if (destroyable != null)
                    {
                        UnityEngine.Object.Destroy(destroyable);
                    }
                }
            });
        }
    }
}
