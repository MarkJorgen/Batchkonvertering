using System;
using dk.gi.app.konto.kontoejerLuk.Application.Models;

namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Runtime
{
    public sealed class SingleInstanceGuard : IDisposable
    {
        private readonly Gi.Batch.Shared.Runtime.SingleInstanceGuard _inner;

        public SingleInstanceGuard(KontoejerLukSettings settings)
        {
            _inner = new Gi.Batch.Shared.Runtime.SingleInstanceGuard(string.IsNullOrWhiteSpace(settings?.MutexName) ? "dk.gi.app.konto.kontoejerLuk" : settings.MutexName);
        }

        public bool TryAcquire()
        {
            return _inner.TryAcquire();
        }

        public void Release()
        {
            _inner.Release();
        }

        public void Dispose()
        {
            _inner.Dispose();
        }
    }
}
