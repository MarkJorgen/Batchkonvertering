using System;
using System.Collections.Generic;
using dk.gi.app.konto.kontoejerLuk.Application.Abstractions;
using dk.gi.app.konto.kontoejerLuk.Application.Models;
using dk.gi.app.konto.kontoejerLuk.Application.Services;
using Gi.Batch.Shared.Logging;

namespace dk.gi.app.konto.kontoejerLuk.Infrastructure.Crm.Dataverse
{
    public sealed class KontoejerLukDataverseWorkflow : IKontoejerLukWorkflow
    {
        private readonly KontoejerLukSettings _settings;
        private readonly IKontoejerLukScanClientFactory _clientFactory;
        private readonly KontoejerLukPlanner _planner;
        private readonly IJobLogger _logger;

        public KontoejerLukDataverseWorkflow(
            KontoejerLukSettings settings,
            IKontoejerLukScanClientFactory clientFactory,
            KontoejerLukPlanner planner,
            IJobLogger logger)
        {
            _settings = settings;
            _clientFactory = clientFactory;
            _planner = planner;
            _logger = logger;
        }

        public KontoejerLukExecutionSummary Execute(KontoejerLukRequest request)
        {
            try
            {
                using (var client = _clientFactory.Create())
                {
                    var deletedAccounts = client.GetDeletedAccounts();
                    int scannedAccounts = deletedAccounts.Count;
                    int openOwners = 0;
                    var closures = new List<AccountOwnerClosure>();

                    foreach (var account in deletedAccounts)
                    {
                        var owners = client.GetOpenOwners(account.AccountId);
                        openOwners += owners.Count;
                        foreach (var closure in _planner.PlanClosures(account, owners))
                        {
                            closures.Add(closure);
                        }
                    }

                    if (_settings.DryRun)
                    {
                        string dryRunMessage = "DRYRUN gennemført. ap_slutdato blev ikke opdateret.";
                        return new KontoejerLukExecutionSummary(true, scannedAccounts, openOwners, closures.Count, dryRunMessage, "local dataverse sdk");
                    }

                    client.ApplyOwnerClosures(closures);
                    return new KontoejerLukExecutionSummary(true, scannedAccounts, openOwners, closures.Count, "Kontoejer-luk workflow gennemført.", "local dataverse sdk");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("KontoejerLuk workflow kastede exception. " + ex.Message);
                return new KontoejerLukExecutionSummary(false, 0, 0, 0, "KontoejerLuk workflow kastede exception: " + ex.Message, ex.GetType().FullName);
            }
        }
    }
}
