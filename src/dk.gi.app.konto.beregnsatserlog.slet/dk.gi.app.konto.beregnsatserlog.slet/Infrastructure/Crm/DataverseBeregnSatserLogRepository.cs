using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Contracts;
using dk.gi.app.konto.beregnsatserlog.slet.Application.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.app.konto.beregnsatserlog.slet.Infrastructure.Crm
{
    public sealed class DataverseBeregnSatserLogRepository : IBeregnSatserLogRepository
    {
        private const string EntityLogicalName = "ap_beregnsatserlog";
        private const string IdAttribute = "ap_beregnsatserlogid";
        private const string CreatedOnAttribute = "createdon";

        private readonly DataverseConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        public DataverseBeregnSatserLogRepository(DataverseConnectionFactory connectionFactory, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public Task<IReadOnlyCollection<BeregnSatserLogRecord>> GetCandidatesAsync(int olderThanYears, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = new List<BeregnSatserLogRecord>();

            using (var client = _connectionFactory.CreateClient())
            {
                if (!client.IsReady)
                {
                    throw new InvalidOperationException(client.LastError ?? "Dataverse-klienten er ikke klar.");
                }

                var query = new QueryExpression(EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(IdAttribute, CreatedOnAttribute),
                    PageInfo = new PagingInfo
                    {
                        Count = 500,
                        PageNumber = 1,
                    }
                };

                query.Criteria.AddCondition(CreatedOnAttribute, ConditionOperator.OlderThanXYears, olderThanYears);

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var page = client.RetrieveMultiple(query);

                    foreach (var entity in page.Entities)
                    {
                        result.Add(new BeregnSatserLogRecord
                        {
                            Id = entity.Id,
                            CreatedOnUtc = entity.Contains(CreatedOnAttribute)
                                ? entity.GetAttributeValue<DateTime?>(CreatedOnAttribute)
                                : null,
                        });
                    }

                    if (!page.MoreRecords)
                    {
                        break;
                    }

                    query.PageInfo.PageNumber += 1;
                    query.PageInfo.PagingCookie = page.PagingCookie;
                }
            }

            _logger.LogInformation(
                "Fandt {CandidateCount} beregnsatserlog-records ældre end {OlderThanYears} år.",
                result.Count,
                olderThanYears);

            return Task.FromResult((IReadOnlyCollection<BeregnSatserLogRecord>)result);
        }

        public Task DeleteAsync(BeregnSatserLogRecord candidate, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var client = _connectionFactory.CreateClient())
            {
                if (!client.IsReady)
                {
                    throw new InvalidOperationException(client.LastError ?? "Dataverse-klienten er ikke klar.");
                }

                client.Delete(EntityLogicalName, candidate.Id);
                _logger.LogInformation("Slettede beregnsatserlog-record {RecordId}", candidate.Id);
            }

            return Task.CompletedTask;
        }
    }
}
