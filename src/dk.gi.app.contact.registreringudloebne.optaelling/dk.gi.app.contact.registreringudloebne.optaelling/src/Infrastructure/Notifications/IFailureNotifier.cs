using System;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Notifications
{
    public interface IFailureNotifier
    {
        void Notify(string subject, string message, Exception exception);
    }
}
