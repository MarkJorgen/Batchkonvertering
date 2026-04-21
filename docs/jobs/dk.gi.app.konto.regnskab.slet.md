# dk.gi.app.konto.regnskab.slet

## Stamdata

- Job: `dk.gi.app.konto.regnskab.slet`
- Iteration: fase 1 startkonvertering efter close-out-forberedelse af `dk.gi.app.konto.satser.slet`
- Legacy-kilde: `GI-Batch/src/dk.gi.app.konto.regnskab.slet`
- Legacy callback analyseret: `Program/Program_App_callback.cs`
- GI-Nugets-kilde analyseret: `KontoRegnskabBLL.IndsaetSletGamleRegnskaber()` og `Ap_KontoManager.HentKontiMedForaeldedeRegnskaber(...)`
- Konverteringsretning: lokal Dataverse-læsning af konti med forældede regnskaber + lokal Service Bus-publicering til `KontoDiv`

## Kort beskrivelse

Legacy-jobbet finder konti med forældede regnskaber, begrænser antallet via CRM-configsettingen `app.konto.regnskab.konti.sletning.koersel` og publicerer derefter ét kø-job pr. konto til `crmpluginjobs` med label `KontoDiv` og payload `Mode=Regnskab`, `action=sletregnskaber`, `ap_konto=<id>`.

Den nye vej er modelleret som:
1. lokal App Configuration-bootstrap og startup-diagnostik
2. lokal Dataverse-læsning af forældelsesfrist fra `ap_kontosystem`
3. lokal query af konti med regnskaber før forældelsesfristen og regnskabsårsag forskellig fra kode `01`
4. batch-begrænsning via CRM `config_configurationsetting`
5. `DRYRUN` uden publicering og `RUN` med Service Bus-publicering

## Arkitekturstatus

- tynd `Program.cs`
- lokal composition root i `ServiceRegistry`
- application-orchestrator uden GI request/response-typer
- infrastructure-adapters for Dataverse, config og Service Bus
- legacy-jobmappen i `src/` er erstattet rent 1:1 af den nye konverterede jobmappe

## Porteret forretningsadfærd i denne iteration

Følgende er porteret fra legacy-koden og GI-Nugets-sporet:
- hentning af konti med forældede regnskaber via local Dataverse-query
- filtrering af slettede konti (`ap_statusframapper != Slettet`)
- filtrering af regnskaber før forældelsesfristen fra `ap_kontosystem.ap_foraeldelsesfrist`
- eksklusion af regnskabsårsag kode `01`
- begrænsning via `app.konto.regnskab.konti.sletning.koersel`
- payload til kø-sporet: `Mode=Regnskab`, `action=sletregnskaber`, `ap_konto=<id>`
- sekventiel forsinkelse mellem publiceringer via `DelayStepSeconds`

## Aktuel status

**Status:** DRYRUN-verificeret close-out-forberedt mellemtilstand.

Det er bevist i denne iteration:
- legacy-jobbet er erstattet rent i `GI-Batch/src`
- ny lokal jobstruktur, settingsfiler og testprojekt er etableret
- lokal hovedvej for kandidatlæsning og Service Bus-publicering er kodet uden GI-requests
- `DRYRUN` på den nye vej er bestået
- startup-diagnostik, CRM-connectivity og Service Bus-settings er verificeret
- observeret resultat i `DRYRUN`: `SelectedAccounts=1`, `Published=0`, `ConnectivityVerified=True`

Det er ikke bevist i denne iteration:
- separat testkørsel er ikke dokumenteret i denne iteration
- `VERIFYCRM` som særskilt mode
- `RUN`
- faktisk queue-effekt

## Autoritativ statusmatrix

| Felt | Status |
|---|---|
| GI NuGet-status | Ny lokal hovedvej etableret; endelig NuGet-close-out ikke dokumenteret i denne iteration |
| GI assembly-status | Ikke verificeret i denne iteration |
| Moderniseringsstatus | Startkonverteret til ny GI-Batch `src`-struktur |
| Afkoblingsstatus | Lokal Dataverse- og Service Bus-hovedvej etableret |
| Clean/arkitekturstatus | Ren erstatning i `src/` uden overlay-rester |
| Shared-status | Genbruger `Gi.Batch.Shared` for generiske CRM-/config-komponenter |
| Config-/deploystatus | `appsettings.local.json`, template og defaults medfølger |
| Build-/teststatus | Lokal exe-build og DRYRUN er bevist; separat testkørsel er ikke dokumenteret i denne iteration |
| DRYRUN-status | Bestået 2026-04-20 (`SelectedAccounts=1`, `Published=0`, `ConnectivityVerified=True`) |
| VERIFYCRM-status | Ikke kørt særskilt i denne iteration |
| RUN-status | Ikke verificeret i denne iteration |
| Restpunkter og undtagelser | Mangler separat testkørsel og kontrolleret queue-publicering i `RUN` |
| Verifikationsgrundlag | Kildeanalyse, GI-Nugets-analyse, ren `src`-erstatningsleverance og dokumenteret DRYRUN-log 2026-04-20 |

## Settingsoversigt

| Setting | Hvor den læses | Brug |
|---|---|---|
| `Mode` | `Program.cs` via `RegnskabSletSettingsFactory` | `VERIFYCRM`, `DRYRUN`, `RUN` |
| `DelayStepSeconds` | `RegnskabSletSettingsFactory` | interval mellem publiceringer i `RUN` |
| `DefaultBatchCount` | `RegnskabSletSettingsFactory` | fallback hvis CRM-config ikke kan læses |
| `CrmConnectionTemplate` | `CrmConnectionStringFactory` | danner Dataverse connection string |
| `CrmServerName` | `CrmConnectionStringFactory` | CRM hostname |
| `CrmClientId` | `CrmConnectionStringFactory` | Client id |
| `CrmClientSecret` | `CrmConnectionStringFactory` | Client secret |
| `CrmAuthority` | `CrmConnectionStringFactory` | Authority/tenant |
| `ServiceBusQueueName` | settings / defaults | kønavn, default `crmpluginjobs` |
| `ServiceBusLabel` | settings / defaults | message label, default `KontoDiv` |
| `ServiceBusBaseUrl`/`ServiceBusSasKeyName`/`ServiceBusSasKey` | lokale overrides eller CRM config | Service Bus-publicering |
| `modtagereEmail` | `RegnskabSletSettingsFactory` | reserveret fejlmailsetting |

## Eksterne kilder og integrationer

| Kilde | Formål | Adfærd | Teknisk adgangsvej | Settings / kilder | Modes | Risici / forbehold |
|---|---|---|---|---|---|---|
| Dataverse / CRM | Finder konti med forældede regnskaber og læser batch-/queue-settings. | Læs | Lokal Dataverse-repository | CRM-settings fra App Configuration / environment | VERIFYCRM, DRYRUN, RUN | Query og configlæsning er teknisk verificeret i DRYRUN; faktisk write/publicering er fortsat ikke verificeret. |
| Service Bus | Publicerer ét kø-job pr. udvalgt konto. | Skriv/publicér | Lokal HTTP-baseret Service Bus-sender | lokale settings eller CRM `config_configurationsetting` | RUN | Service Bus-settings er verificeret i DRYRUN; faktisk publicering er ikke verificeret i denne iteration. |
| Azure App Configuration / environment | Leverer bootstrap og CRM-settings. | Læs | Settings loader + Azure App Configuration | `AZURE_APPCONFIG_CONNECTIONSTRING` og miljøkilder | Alle modes | Systemforvaltede værdier er ikke hardcodet lokalt. |

## Næste anbefalede step

1. Dokumentér separat testkørsel, hvis den ønskes som supplerende evidens.
2. Kør særskilt `VERIFYCRM`, hvis der ønskes separat connectivity-evidens.
3. Kør kontrolleret `RUN`, hvis queue-publicering er driftsmæssigt forsvarlig.
4. Opdatér statusmatrix og evidens efter første reelle kø-publicering.

## Testcases for eksterne kald

Særskilt testcase-dokument: `docs/jobs/testcases/dk.gi.app.konto.regnskab.slet.external-calls.md`
