using System;
using dk.gi.app.contact.registrering.optaelling.Application.Models;

namespace dk.gi.app.contact.registrering.optaelling.Application.Abstractions
{
    public interface IContactRegistreringWorkflowClient : IDisposable
    {
        ContactRegistreringExecutionSummary VerifyConnection();
        ContactRegistreringExecutionSummary CloseExpiredTreklipOwnerRegistrations();
        ContactRegistreringExecutionSummary CreateJobsForContactRegistrerings(Guid? registreringId);
    }
}
