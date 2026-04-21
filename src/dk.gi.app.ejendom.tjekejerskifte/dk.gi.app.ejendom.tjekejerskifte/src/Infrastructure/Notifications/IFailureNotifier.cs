using System;

namespace dk.gi.app.ejendom.tjekejerskifte.Infrastructure.Notifications
{
    public interface IFailureNotifier
    {
        void Notify(string subject, string body, Exception exception);
    }
}
