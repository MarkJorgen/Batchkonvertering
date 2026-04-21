using System;
using dk.gi.app.contact.lassox.ophoer.Application.Models;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Notifications
{
    public sealed class FailureNotificationService
    {
        private readonly Gi.Batch.Shared.Notifications.FailureNotificationService _inner;

        public FailureNotificationService(LassoXOphoerSettings settings, IFailureNotifier notifier)
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
