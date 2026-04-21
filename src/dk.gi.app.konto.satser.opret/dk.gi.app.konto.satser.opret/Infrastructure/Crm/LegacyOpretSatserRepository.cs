using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dk.gi.app.konto.satser.opret.Application.Contracts;
using dk.gi.crm.data.bll;
using dk.gi.crm.giproxy;
using dk.gi.crm.managers.V2;
using dk.gi.crm.request.V2;
using dk.gi.crm.response.V2;
using Microsoft.Extensions.Logging;

namespace dk.gi.app.konto.satser.opret.Infrastructure.Crm
{
    public sealed class LegacyOpretSatserRepository : IOpretSatserRepository
    {
        private readonly DataverseConnectionFactory _connectionFactory;
        private readonly ILogger _logger;

        public LegacyOpretSatserRepository(DataverseConnectionFactory connectionFactory, ILogger logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public Task<int> GetCandidateCountAsync(int satsAar, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var crmcontext = _connectionFactory.CreateLegacyContext())
            using (var kontoManager = new Ap_KontoManager(crmcontext))
            using (var reguleringsprocentManager = new AP_reguleringsprocentManager(crmcontext))
            using (var satserManager = new Ap_SatserManager(crmcontext))
            {
                var reguleringsprocenter = reguleringsprocentManager
                    .HentAlle()
                    .Where(rp => rp.AP_Startdato.Value.ToLocalTimeGI().Year == satsAar)
                    .ToList();

                if (reguleringsprocenter.Count == 0)
                {
                    throw new InvalidOperationException($"Reguleringsprocent for {satsAar} blev ikke fundet!");
                }

                if (reguleringsprocenter.Count > 1)
                {
                    throw new InvalidOperationException($"Reguleringsprocent for {satsAar} blev fundet flere gange!");
                }

                if (satserManager.ErSatserOprettetTilAar(satsAar))
                {
                    throw new InvalidOperationException($"Satser for {satsAar} er allerede oprettet!");
                }

                var count = kontoManager
                    .HentAlleStatusAktivEllerBero(
                        AP_konto.Fields.Id,
                        AP_konto.Fields.AP_Kontonr,
                        AP_konto.Fields.AP_Bindingstype,
                        AP_konto.Fields.ap_lovgrundlag)
                    .Count(k =>
                        k.AP_Bindingstype.GetValueOrDefault() == false &&
                        k.ap_lovgrundlag.GetValueOrDefault() == (int)AP_konto_ap_lovgrundlag.Førjuli2015);

                return Task.FromResult(count);
            }
        }

        public Task<int> CreateAsync(int satsAar, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using (var crmcontext = _connectionFactory.CreateLegacyContext())
            using (var kontoManager = new Ap_KontoManager(crmcontext))
            using (var reguleringsprocentManager = new AP_reguleringsprocentManager(crmcontext))
            using (var satserManager = new Ap_SatserManager(crmcontext))
            {
                var konti = kontoManager
                    .HentAlleStatusAktivEllerBero(
                        AP_konto.Fields.Id,
                        AP_konto.Fields.AP_Kontonr,
                        AP_konto.Fields.AP_Bindingstype,
                        AP_konto.Fields.ap_lovgrundlag)
                    .Where(k =>
                        k.AP_Bindingstype.GetValueOrDefault() == false &&
                        k.ap_lovgrundlag.GetValueOrDefault() == (int)AP_konto_ap_lovgrundlag.Førjuli2015)
                    .OrderBy(k => k.AP_Kontonr)
                    .ToList();

                var reguleringsprocenter = reguleringsprocentManager
                    .HentAlle()
                    .Where(rp => rp.AP_Startdato.Value.ToLocalTimeGI().Year == satsAar)
                    .ToList();

                if (reguleringsprocenter.Count == 0)
                {
                    throw new InvalidOperationException($"Reguleringsprocent for {satsAar} blev ikke fundet!");
                }

                if (reguleringsprocenter.Count > 1)
                {
                    throw new InvalidOperationException($"Reguleringsprocent for {satsAar} blev fundet flere gange!");
                }

                if (satserManager.ErSatserOprettetTilAar(satsAar))
                {
                    throw new InvalidOperationException($"Satser for {satsAar} er allerede oprettet!");
                }

                var reguleringsprocent = reguleringsprocenter.First();
                var beregnSatsRequest = new BeregnSatsRequest(crmcontext);
                var createdCount = 0;

                foreach (var konto in konti)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    _logger.LogInformation("Opretter sats for {Kontonr}", konto.AP_Kontonr);

                    beregnSatsRequest.BatchNr = Guid.NewGuid().ToString().Substring(0, 20);
                    beregnSatsRequest.Beregningslog = false;
                    beregnSatsRequest.Reguleringsprocent = reguleringsprocent;
                    beregnSatsRequest.ReguleringsprocentId = reguleringsprocent.AP_reguleringsprocentId.Value;
                    beregnSatsRequest.Genberegning = false;
                    beregnSatsRequest.Konto = konto;
                    beregnSatsRequest.KontoNr = konto.AP_Kontonr;

                    BeregnSatsResponse response = beregnSatsRequest.Execute<BeregnSatsResponse>();
                    if (!response.Status.IsOK())
                    {
                        throw new InvalidOperationException("beregnSatsRequest fejlede eller timede ud - dk.gi.app.konto.satser.opret skal køres igen - tjek log");
                    }

                    createdCount++;
                }

                return Task.FromResult(createdCount);
            }
        }
    }
}
