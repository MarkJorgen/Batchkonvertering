using System;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Runtime
{
    public sealed class SingleInstanceGuard : IDisposable
    {
        private readonly Gi.Batch.Shared.Runtime.SingleInstanceGuard _inner;

        public SingleInstanceGuard(ContactRegistreringUdloebneOptaellingSettings settings)
        {
            if (settings == null) throw new System.ArgumentNullException(nameof(settings));
            _inner = new Gi.Batch.Shared.Runtime.SingleInstanceGuard(settings?.MutexName);
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
