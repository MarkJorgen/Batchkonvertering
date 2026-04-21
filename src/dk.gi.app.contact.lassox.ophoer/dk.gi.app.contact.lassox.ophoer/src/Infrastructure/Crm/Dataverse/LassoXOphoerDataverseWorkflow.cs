using System;
using System.Linq;
using dk.gi.app.contact.lassox.ophoer.Application.Abstractions;
using dk.gi.app.contact.lassox.ophoer.Application.Models;
using dk.gi.app.contact.lassox.ophoer.Application.Services;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Crm
{
    public sealed class LassoXOphoerDataverseWorkflow : ILassoXOphoerWorkflow
    {
        private readonly ILassoXOphoerScanClientFactory _clientFactory;
        private readonly LassoXOphoerDecisionEngine _decisionEngine;
        private readonly IJobLogger _logger;

        public LassoXOphoerDataverseWorkflow(
            ILassoXOphoerScanClientFactory clientFactory,
            LassoXOphoerDecisionEngine decisionEngine,
            IJobLogger logger)
        {
            _clientFactory = clientFactory;
            _decisionEngine = decisionEngine;
            _logger = logger;
        }

        public LassoXOphoerExecutionSummary Execute(LassoXOphoerRequest request)
        {
            try
            {
                using (var client = _clientFactory.Create())
                {
                    _logger.Info("Dataverse klient klar. Starter læsning af LassoX-kontakter.");

                    _logger.Info("Starter query: kontakter med aktivt LassoX-abonnement.");
                    var contacts = client.GetContactsWithActiveSubscription(request.ContactId);
                    _logger.Info("Færdig query: kontakter med aktivt LassoX-abonnement. Antal=" + contacts.Count);

                    _logger.Info("Starter query: åbne kontoejere.");
                    var openAccountOwnerIds = client.GetOpenAccountOwnerContactIds();
                    _logger.Info("Færdig query: åbne kontoejere. Antal=" + openAccountOwnerIds.Count);

                    _logger.Info("Starter query: reelle ejere.");
                    var realOwnerIds = client.GetRealOwnerContactIds();
                    _logger.Info("Færdig query: reelle ejere. Antal=" + realOwnerIds.Count);

                    _logger.Info("Starter beslutningsfase. Input: contacts=" + contacts.Count + ", openOwners=" + openAccountOwnerIds.Count + ", realOwners=" + realOwnerIds.Count + ".");
                    var decisions = _decisionEngine.Decide(contacts, openAccountOwnerIds, realOwnerIds);

                    int keptByOpenOwner = decisions.Count(x => x.KeepSubscription && x.Reason.Contains("kontoejer"));
                    int keptByRealOwner = decisions.Count(x => x.KeepSubscription && x.Reason.Contains("reel ejer"));
                    _logger.Info("Beslutningsfase færdig. BeholdtSomKontoejer=" + keptByOpenOwner + ", BeholdtSomReelEjer=" + keptByRealOwner + ", KandidaterTilAfmelding=" + decisions.Count(x => !x.KeepSubscription) + ".");

                    var unsubscribeIds = decisions
                        .Where(decision => !decision.KeepSubscription)
                        .Select(decision => decision.ContactId)
                        .Distinct()
                        .ToList();

                    int updated = 0;
                    if (request.WriteChanges && unsubscribeIds.Count > 0)
                    {
                        _logger.Info("Starter write-fase for " + unsubscribeIds.Count + " kontakt(er).");
                        updated = client.UnsubscribeContacts(unsubscribeIds);
                        _logger.Info("Write-fase færdig. Opdateret=" + updated + ".");
                    }
                    else if (request.WriteChanges)
                    {
                        _logger.Info("Write-fase overflødig. Ingen kontakter krævede afmelding.");
                    }
                    else
                    {
                        _logger.Info("Simulation aktiv. Ingen CRM-opdateringer udføres.");
                    }

                    string message = request.WriteChanges
                        ? "LassoX workflow gennemført med eksplicit write-mode."
                        : "LassoX workflow gennemført i eksplicit simulation uden CRM-opdateringer.";

                    _logger.Info("Workflow gennemført. Scannet=" + contacts.Count + ", Kandidater=" + unsubscribeIds.Count + ", Opdateret=" + updated + ", Mode=" + (request.WriteChanges ? "RUN" : "DRYRUN") + ".");

                    return new LassoXOphoerExecutionSummary(
                        true,
                        contacts.Count,
                        unsubscribeIds.Count,
                        updated,
                        message,
                        "local dataverse adapter");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("LassoX workflow kastede exception. " + ex.Message);
                return new LassoXOphoerExecutionSummary(false, 0, 0, 0, "LassoX workflow kastede exception: " + ex.Message, ex.GetType().FullName);
            }
        }
    }
}
