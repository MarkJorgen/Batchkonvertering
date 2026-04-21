using System;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Notifications
{
    public sealed class ConsoleFailureNotifier : IFailureNotifier
    {
        private readonly Gi.Batch.Shared.Notifications.ConsoleFailureNotifier _inner = new Gi.Batch.Shared.Notifications.ConsoleFailureNotifier();

        public void Notify(string subject, string message, Exception exception)
        {
            _inner.Notify(subject, message, exception);
        }
    }
}
