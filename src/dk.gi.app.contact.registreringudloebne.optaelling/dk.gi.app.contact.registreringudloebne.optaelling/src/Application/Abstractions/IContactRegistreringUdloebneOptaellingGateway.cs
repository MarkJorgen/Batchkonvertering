using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions
{
    public interface IContactRegistreringUdloebneOptaellingGateway
    {
        ContactRegistreringUdloebneExecutionSummary Execute(ContactRegistreringUdloebneOptaellingRequest request);
    }
}
