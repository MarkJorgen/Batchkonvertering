using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.satser.slet.Application.Contracts;
using dk.gi.app.konto.satser.slet.Application.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk.Query;

namespace dk.gi.app.konto.satser.slet.Infrastructure.Crm
{
    public sealed class DataverseSatserRepository : ISatserRepository
    {
        private const string EntityLogicalName = "ap_satser";
        private const string IdAttribute = "ap_satserid";
        private const string StartdatoAttribute = "ap_startdato";
        private const string UndtagelseAttribute = "ap_undtagelse";
        private const string StateCodeAttribute = "statecode";
        private const int ActiveStateCode = 0;

        private readonly DataverseConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        public DataverseSatserRepository(DataverseConnectionFactory connectionFactory, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public Task<IReadOnlyCollection<SatserRecord>> GetCandidatesAsync(int satsAar, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = new List<SatserRecord>();
            var lowerBound = new DateTime(satsAar, 1, 1).AddDays(-1);

            using (var client = _connectionFactory.CreateClient())
            {
                if (!client.IsReady)
                {
                    throw new InvalidOperationException(client.LastError ?? "Dataverse-klienten er ikke klar.");
                }

                var query = new QueryExpression(EntityLogicalName)
                {
                    ColumnSet = new ColumnSet(IdAttribute, StartdatoAttribute, UndtagelseAttribute),
                    PageInfo = new PagingInfo
                    {
                        Count = 500,
                        PageNumber = 1,
                    }
                };

                query.Criteria.AddCondition(StartdatoAttribute, ConditionOperator.GreaterEqual, lowerBound);
                query.Criteria.AddCondition(StateCodeAttribute, ConditionOperator.Equal, ActiveStateCode);

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var page = client.RetrieveMultiple(query);

                    foreach (var entity in page.Entities)
                    {
                        var startdato = entity.GetAttributeValue<DateTime?>(StartdatoAttribute)?.ToLocalTime();
                        var undtagelse = entity.GetAttributeValue<bool?>(UndtagelseAttribute).GetValueOrDefault();

                        if (startdato.HasValue && startdato.Value.Year == satsAar && !undtagelse)
                        {
                            result.Add(new SatserRecord
                            {
                                Id = entity.Id,
                                StartdatoLocal = startdato,
                                IsUndtagelse = undtagelse,
                            });
                        }
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
                "Fandt {CandidateCount} ap_satser-records til sletning for år {SatsAar}.",
                result.Count,
                satsAar);

            return Task.FromResult((IReadOnlyCollection<SatserRecord>)result);
        }

        public Task DeleteAsync(SatserRecord candidate, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var client = _connectionFactory.CreateClient())
            {
                if (!client.IsReady)
                {
                    throw new InvalidOperationException(client.LastError ?? "Dataverse-klienten er ikke klar.");
                }

                client.Delete(EntityLogicalName, candidate.Id);
                _logger.LogInformation("Slettede ap_satser-record {RecordId}", candidate.Id);
            }

            return Task.CompletedTask;
        }
    }
}
