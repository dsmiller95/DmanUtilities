using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dman.Utilities
{
    public class ExclusiveAccessResource
    {
        public bool isLocked { get; private set; }

        private class Lock : IDisposable
        {
            private ExclusiveAccessResource resource;
            private bool isDisposed;

            public Lock(ExclusiveAccessResource resource)
            {
                this.resource = resource;
                if (this.resource.isLocked)
                {
                    throw new Exception("attempted to open a lock on an already locked resource");
                }
                this.resource.isLocked = true;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!isDisposed)
                {
                    if (disposing)
                    {
                        resource.isLocked = false;
                        // TODO: dispose managed state (managed objects)
                    }
                    resource = null;

                    // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                    // TODO: set large fields to null
                    isDisposed = true;
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        public IDisposable TakeLock()
        {
            return new Lock(this);
        }
    }
}
