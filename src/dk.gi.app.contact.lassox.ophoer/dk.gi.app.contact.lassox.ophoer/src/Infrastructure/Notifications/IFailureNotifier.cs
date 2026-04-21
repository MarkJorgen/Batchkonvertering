using System;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Notifications
{
    public interface IFailureNotifier
    {
        void Notify(string subject, string message, Exception exception);
    }
}
