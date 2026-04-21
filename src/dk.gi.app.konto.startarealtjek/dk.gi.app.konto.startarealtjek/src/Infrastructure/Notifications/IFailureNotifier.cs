using System;

namespace dk.gi.app.konto.startarealtjek.Infrastructure.Notifications
{
    public interface IFailureNotifier
    {
        void Notify(string subject, string message, Exception exception);
    }
}
