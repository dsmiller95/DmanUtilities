using System;

namespace Dman.SaveSystem
{
    public interface IAmKeptAlive
    {
        public IKeepAliveHandle KeepAliveUntil();
    }
    
    public interface IKeepAliveHandle : IDisposable
    {
        
    }
    
    public class KeepAliveContainer: IAmKeptAlive
    {
        private readonly Action _onDispose;
        private int _totalKeptAlive = 0;
        private bool _isReadyToDispose = false;
        private bool _isDisposed = false;
        
        public KeepAliveContainer(Action onDispose)
        {
            _onDispose = onDispose;
        }


        public IKeepAliveHandle KeepAliveUntil()
        {
            if(_isDisposed) throw new ObjectDisposedException("KeepAliveContainer");
            _totalKeptAlive++;
            return new KeepAliveHandle(this);
        }
        
        private void OnHandleDisposed()
        {
            if(_isDisposed) throw new ObjectDisposedException("KeepAliveContainer");
            _totalKeptAlive--;
            TryDispose();
        }

        // TODO: may not need this. could just take an extra keep alive handle
        public void SetReadyToDispose()
        {
            if(_isDisposed) throw new ObjectDisposedException("KeepAliveContainer");
            this._isReadyToDispose = true;
            TryDispose();
        }

        private void TryDispose()
        {
            if(_isDisposed) return;
            if (_totalKeptAlive > 0 || !_isReadyToDispose) return;
            
            _isDisposed = true;
            _onDispose();
        }

        private class KeepAliveHandle : IKeepAliveHandle
        {
            private readonly KeepAliveContainer _container;
            public KeepAliveHandle(KeepAliveContainer container) => _container = container;
            public void Dispose() => _container.OnHandleDisposed();
        }
    }
}