using System;
using dk.gi.app.contact.registreringudloebne.optaelling.Application.Models;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Application.Abstractions
{
    public interface IContactRegistreringUdloebneWorkflow
    {
        ContactRegistreringUdloebneExecutionSummary Execute(Guid? registreringId);
    }
}
