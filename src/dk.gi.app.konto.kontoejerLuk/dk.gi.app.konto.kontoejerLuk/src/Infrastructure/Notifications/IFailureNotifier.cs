using System;

namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Notifications
{
    public interface IFailureNotifier
    {
        void Notify(string subject, string body, Exception exception);
    }
}
