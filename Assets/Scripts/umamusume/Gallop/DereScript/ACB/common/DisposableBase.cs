using System;

namespace DereTore.Common {
    public abstract class DisposableBase : IDisposable {

        protected DisposableBase() {
            _isDisposed = false;
        }

        public void Dispose() {
            if (_isDisposed) {
                return;
            }
            Dispose(true);
            _isDisposed = true;
        }

        public bool IsDisposed { get { return _isDisposed; } }

        protected abstract void Dispose(bool disposing);

        private bool _isDisposed;

        ~DisposableBase() {
            if (_isDisposed) {
                return;
            }
            Dispose(false);
            _isDisposed = true;
        }

    }
}
