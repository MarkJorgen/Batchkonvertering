using System;

namespace Gi.Batch.Shared.Notifications
{
    public sealed class ConsoleFailureNotifier
    {
        public void Notify(string subject, string message, Exception exception)
        {
            Console.Error.WriteLine("[FAILURE] " + subject);
            Console.Error.WriteLine(message ?? string.Empty);
            if (exception != null)
            {
                Console.Error.WriteLine(exception);
            }
        }
    }
}
