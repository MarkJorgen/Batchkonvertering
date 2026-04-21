using System;
using dk.gi.app.contact.selskab.Application.Models;

namespace dk.gi.app.contact.selskab.Infrastructure.Notifications
{
    public sealed class FailureNotificationService
    {
        private readonly Gi.Batch.Shared.Notifications.FailureNotificationService _inner;

        public FailureNotificationService(ContactSelskabSettings settings, IFailureNotifier notifier)
        {
            if (notifier == null) throw new ArgumentNullException(nameof(notifier));
            _inner = new Gi.Batch.Shared.Notifications.FailureNotificationService(
                settings?.FailureRecipients,
                notifier.Notify);
        }

        public void NotifyFailure(string subject, string message, Exception exception)
        {
            _inner.NotifyFailure(subject, message, exception);
        }
    }
}
