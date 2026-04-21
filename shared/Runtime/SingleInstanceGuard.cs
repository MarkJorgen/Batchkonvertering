using System;
using System.Diagnostics;
using System.Threading;

namespace Gi.Batch.Shared.Runtime
{
    public sealed class SingleInstanceGuard : IDisposable
    {
        private readonly string _mutexName;
        private Mutex _mutex;
        private bool _lockTaken;

        public SingleInstanceGuard(string mutexName)
        {
            if (string.IsNullOrWhiteSpace(mutexName))
            {
                throw new ArgumentException("MutexName must be set.", nameof(mutexName));
            }

            _mutexName = mutexName;
        }

        public bool TryAcquire()
        {
            if (_lockTaken)
            {
                return true;
            }

            try
            {
                _mutex = new Mutex(false, _mutexName);
                bool acquired;
                try
                {
                    acquired = _mutex.WaitOne(0);
                }
                catch (AbandonedMutexException)
                {
                    acquired = true;
                }

                _lockTaken = acquired;
                return acquired;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Failed to acquire mutex '{0}': {1}", _mutexName, ex);
                _mutex?.Dispose();
                _mutex = null;
                _lockTaken = false;
                return false;
            }
        }

        public void Release()
        {
            if (_mutex == null || !_lockTaken)
            {
                return;
            }

            try
            {
                _mutex.ReleaseMutex();
            }
            catch (Exception ex)
            {
                Trace.TraceWarning("Error releasing mutex '{0}': {1}", _mutexName, ex);
            }
            finally
            {
                _mutex.Dispose();
                _mutex = null;
                _lockTaken = false;
            }
        }

        public void Dispose()
        {
            Release();
        }
    }
}
