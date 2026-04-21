using dk.gi.app.contact.registrering.optaelling.Application.Models;

namespace dk.gi.app.contact.registrering.optaelling.Application.Abstractions
{
    public interface IContactRegistreringOptaellingGateway
    {
        ContactRegistreringExecutionSummary Execute(ContactRegistreringOptaellingRequest request);
    }
}
