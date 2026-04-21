using System;
using JobConfiguration = Gi.Batch.Shared.Configuration.JobConfiguration;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Notifications
{
    public sealed class ConfigurableFailureNotifier : IFailureNotifier
    {
        private readonly Action<string, string, Exception> _notify;

        public ConfigurableFailureNotifier(JobConfiguration configuration, string failureRecipients)
        {
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            bool enableFailureEmail = configuration.GetBool("EnableFailureEmail", false);
            if (enableFailureEmail)
            {
                var emailNotifier = new Gi.Batch.Shared.Notifications.EmailFailureNotifier(configuration, failureRecipients);
                _notify = emailNotifier.Notify;
            }
            else
            {
                var consoleNotifier = new Gi.Batch.Shared.Notifications.ConsoleFailureNotifier();
                _notify = consoleNotifier.Notify;
            }
        }

        public void Notify(string subject, string message, Exception exception)
        {
            _notify(subject, message, exception);
        }
    }
}
