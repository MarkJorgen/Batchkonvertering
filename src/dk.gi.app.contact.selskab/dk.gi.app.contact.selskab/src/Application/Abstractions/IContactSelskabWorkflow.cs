using dk.gi.app.contact.selskab.Application.Models;

namespace dk.gi.app.contact.selskab.Application.Abstractions
{
    public interface IContactSelskabWorkflow
    {
        ContactSelskabExecutionSummary Execute(ContactSelskabRequest request);
    }
}
