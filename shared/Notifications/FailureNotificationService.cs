using System;

namespace Gi.Batch.Shared.Notifications
{
    public sealed class FailureNotificationService
    {
        private readonly string _failureRecipients;
        private readonly Action<string, string, Exception> _notify;

        public FailureNotificationService(string failureRecipients, Action<string, string, Exception> notify)
        {
            _failureRecipients = failureRecipients ?? string.Empty;
            _notify = notify ?? throw new ArgumentNullException(nameof(notify));
        }

        public void NotifyFailure(string subject, string message, Exception exception)
        {
            string decorated = string.IsNullOrWhiteSpace(_failureRecipients)
                ? (message ?? string.Empty) + Environment.NewLine + "modtagereEmail er ikke sat endnu."
                : (message ?? string.Empty) + Environment.NewLine + "Modtagere: " + _failureRecipients;

            _notify(subject, decorated, exception);
        }
    }
}
