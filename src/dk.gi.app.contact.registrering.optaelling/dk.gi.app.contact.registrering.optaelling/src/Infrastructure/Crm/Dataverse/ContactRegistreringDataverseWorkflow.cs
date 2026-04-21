using System;
using dk.gi.app.contact.registrering.optaelling.Application.Abstractions;
using dk.gi.app.contact.registrering.optaelling.Application.Models;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm
{
    public sealed class ContactRegistreringDataverseWorkflow : IContactRegistreringWorkflow
    {
        private readonly IContactRegistreringWorkflowClientFactory _clientFactory;
        private readonly IJobLogger _logger;

        public ContactRegistreringDataverseWorkflow(
            IContactRegistreringWorkflowClientFactory clientFactory,
            IJobLogger logger)
        {
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public ContactRegistreringExecutionSummary Execute(Guid? registreringId)
        {
            string scope = registreringId.HasValue ? registreringId.Value.ToString() : "alle registreringer";

            try
            {
                _logger.Info("Opretter lokal Dataverse-klient til ContactRegistrering-workflow. Scope=" + scope);

                using (var client = _clientFactory.Create())
                {
                    _logger.Info("Kalder CloseExpiredTreklipOwnerRegistrations().");
                    ContactRegistreringExecutionSummary closeExpiredResult = client.CloseExpiredTreklipOwnerRegistrations();
                    if (closeExpiredResult.Success == false)
                    {
                        return Failure(
                            closeExpired: false,
                            createJobs: false,
                            message: Prefix("Lukning af afsluttede treklip/ejerregistreringer fejlede.", closeExpiredResult.Message),
                            source: closeExpiredResult.Source);
                    }

                    _logger.Info("Kalder CreateJobsForContactRegistrerings(). Scope=" + scope);
                    ContactRegistreringExecutionSummary createJobsResult = client.CreateJobsForContactRegistrerings(registreringId);
                    if (createJobsResult.Success == false)
                    {
                        return Failure(
                            closeExpired: true,
                            createJobs: false,
                            message: Prefix("Dannelse af kontaktjobs fejlede.", createJobsResult.Message),
                            source: createJobsResult.Source);
                    }
                }

                _logger.Info("ContactBLL-erstatning gennemført. Scope=" + scope);
                return new ContactRegistreringExecutionSummary(
                    success: true,
                    closedExpiredTreklipOwnerRegistrations: true,
                    createdJobsForContacts: true,
                    message: registreringId.HasValue
                        ? "ContactBLL-erstatning gennemført for specifik registrering."
                        : "ContactBLL-erstatning gennemført for alle registreringer.",
                    source: "local dataverse adapter");
            }
            catch (Exception ex)
            {
                _logger.Error("ContactBLL-erstatning kastede exception. " + ex.Message);
                return Failure(
                    closeExpired: false,
                    createJobs: false,
                    message: "ContactBLL-erstatning kastede exception: " + ex.Message,
                    source: ex.GetType().FullName);
            }
        }

        private ContactRegistreringExecutionSummary Failure(bool closeExpired, bool createJobs, string message, string source)
        {
            _logger.Warning(message);
            return new ContactRegistreringExecutionSummary(
                success: false,
                closedExpiredTreklipOwnerRegistrations: closeExpired,
                createdJobsForContacts: createJobs,
                message: message,
                source: source);
        }

        private static string Prefix(string prefix, string details)
        {
            if (string.IsNullOrWhiteSpace(details))
            {
                return prefix;
            }

            return prefix + " " + details;
        }
    }
}
