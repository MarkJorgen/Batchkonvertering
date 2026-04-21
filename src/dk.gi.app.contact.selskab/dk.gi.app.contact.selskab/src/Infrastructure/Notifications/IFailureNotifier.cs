using System;

namespace dk.gi.app.contact.selskab.Infrastructure.Notifications
{
    public interface IFailureNotifier
    {
        void Notify(string subject, string message, Exception exception);
    }
}
