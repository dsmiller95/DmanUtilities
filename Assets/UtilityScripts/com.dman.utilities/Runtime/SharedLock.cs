using System;

namespace Dman.Utilities
{
    public class SharedLock
    {
        public bool IsLocked => TotalLocks > 0;
        public int TotalLocks { get; private set; }

        private class Lock : IDisposable
        {
            private SharedLock resource;
            private bool isDisposed = false;

            public Lock(SharedLock resource)
            {
                this.resource = resource;
                this.resource.TotalLocks++;
            }

            private void ReleaseUnmanagedResources()
            {
                if (isDisposed) return;
                isDisposed = true;
                this.resource.TotalLocks--;
            }

            public void Dispose()
            {
                ReleaseUnmanagedResources();
                GC.SuppressFinalize(this);
            }

            ~Lock() { ReleaseUnmanagedResources(); }
        }

        public IDisposable TakeLock()
        {
            return new Lock(this);
        }
    }
}
