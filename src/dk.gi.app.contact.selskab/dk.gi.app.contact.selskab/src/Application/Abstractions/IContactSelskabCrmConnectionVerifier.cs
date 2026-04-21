using dk.gi.app.contact.selskab.Application.Models;

namespace dk.gi.app.contact.selskab.Application.Abstractions
{
    public interface IContactSelskabCrmConnectionVerifier
    {
        ContactSelskabExecutionSummary Verify();
    }
}
