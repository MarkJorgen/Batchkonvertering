# dk.gi.app.konto.satser.slet

## Stamdata

- Job: `dk.gi.app.konto.satser.slet`
- Iteration: fase 1 startkonvertering i GI-Batch `src`-erstatningsmodellen
- Legacy-kilde: `GI-Batch/src/dk.gi.app.konto.satser.slet`
- Legacy callback analyseret: `Program/Program_App_callback.cs`
- GI-Nugets-kilde analyseret: `Ap_SatserManager.HentSatserIdForSletTilAar(int aar)`
- Konverteringsretning: lokal Dataverse-læsning + lokal delete-adapter for `ap_satser`

## Kort beskrivelse

Legacy-jobbet finder aktive `ap_satser` for næste sats-år, filtrerer til records hvor `ap_undtagelse = false`, og sletter dem derefter én for én.

Den nye vej er modelleret som:
1. lokal App Configuration-bootstrap og startup-diagnostik
2. lokal Dataverse-læsning af `ap_satser`
3. lokal filtrering på `SatsAar`, `statecode = Active` og `ap_undtagelse = false`
4. delete af kandidat-records i `RUN`
5. `VERIFYCRM` og `DRYRUN` uden sletning

## Arkitekturstatus

- tynd `Program.cs`
- lokal composition root i `ServiceRegistry`
- application-orchestrator uden GI request/response-typer
- infrastructure-adapters for Dataverse, config, runtime guard og failure notification
- legacy-jobmappen i `src/` er erstattet rent 1:1 af den nye konverterede jobmappe

## Porteret forretningsadfærd i denne iteration

Følgende er porteret fra legacy-koden og GI-Nugets-sporet:
- sats-år beregnes/konfigureres via `SatsAar` (default næste kalenderår)
- query på `ap_satser` med kolonnerne `ap_satserid`, `ap_startdato` og `ap_undtagelse`
- filter på `ap_startdato >= 31/12 året før`, `statecode = Active`
- lokal efterfiltrering så kun records i det ønskede år og uden undtagelsesflag slettes
- delete pr. kandidat i `RUN`

## Aktuel status

**Status:** DRYRUN-verificeret close-out-forberedt mellemtilstand.

Det er bevist i denne iteration:
- legacy-jobbet er erstattet rent i `GI-Batch/src`
- ny lokal jobstruktur er etableret
- query-/delete-logikken er porteret væk fra GI manager-sporet i ny kode
- `appsettings.local.json`, template og testprojekt medfølger

Det er ikke bevist i denne iteration:
- lokal testkørsel er ikke dokumenteret i denne iteration
- `VERIFYCRM`
- `RUN`
- faktisk sletteeffekt

## Autoritativ statusmatrix

| Felt | Status |
|---|---|
| GI NuGet-status | Ny lokal hovedvej etableret; endelig NuGet-close-out ikke dokumenteret i denne iteration |
| GI assembly-status | Ikke verificeret i denne iteration |
| Moderniseringsstatus | Startkonverteret til ny GI-Batch-struktur |
| Afkoblingsstatus | Lokal Dataverse-hovedvej etableret for læsning og delete |
| Clean/arkitekturstatus | Ren erstatning i `src/` uden overlay-rester |
| Shared-status | Genbruger `Gi.Batch.Shared` for generiske runtime/config/failure-komponenter |
| Config-/deploystatus | `appsettings.local.json` og template medfølger |
| Build-/teststatus | Lokal exe-build og DRYRUN er bevist; separat testkørsel er ikke dokumenteret i denne iteration |
| DRYRUN-status | Bestået 2026-04-20 (`Candidates=1`, `Deleted=0`, `ConnectivityVerified=True`) |
| VERIFYCRM-status | Ikke kørt særskilt i denne iteration |
| RUN-status | Ikke verificeret i denne iteration |
| Restpunkter og undtagelser | Mangler lokal build/test, connectivity og kontrolleret sletteverifikation |
| Verifikationsgrundlag | Kildeanalyse, GI-Nugets-analyse, ren `src`-erstatningsleverance og dokumenteret DRYRUN-log 2026-04-20 |

## Settingsoversigt

| Setting | Hvor den læses | Brug |
|---|---|---|
| `Mode` | `Program.cs` via `SletSatserSettingsFactory` | `VERIFYCRM`, `DRYRUN`, `RUN` |
| `SatsAar` | `SletSatserSettingsFactory` | Målsatser for det år der skal ryddes |
| `TimeOutMinutter` | `SletSatserSettingsFactory` | Legacy-parameter bevaret til senere runtime-tuning |
| `SecondsToSleep` | `SletSatserSettingsFactory` | Legacy sync-/mutex-parameter bevaret |
| `MaxWaitCount` | `SletSatserSettingsFactory` | Legacy sync-/mutex-parameter bevaret |
| `CrmConnectionTemplate` | `CrmConnectionStringFactory` | Danner Dataverse connection string |
| `CrmServerName` | `CrmConnectionStringFactory` | CRM hostname |
| `CrmClientId` | `CrmConnectionStringFactory` | Client id |
| `CrmClientSecret` | `CrmConnectionStringFactory` | Client secret |
| `CrmAuthority` | `CrmConnectionStringFactory` | Authority/tenant |
| `modtagereEmail` | `SletSatserSettingsFactory` | Reserveret fejlmailsetting |

## Eksterne kilder og integrationer

| Kilde | Formål | Adfærd | Teknisk adgangsvej | Settings / kilder | Modes | Risici / forbehold |
|---|---|---|---|---|---|---|
| Dataverse / CRM | Finder og sletter `ap_satser` for målåret uden undtagelsesflag. | Læs i DRYRUN, læs + delete i RUN | Lokal Dataverse-repository | CRM-settings fra App Configuration / environment | VERIFYCRM, DRYRUN, RUN | Sletning er ikke valideret i denne iteration. |
| Azure App Configuration / environment | Leverer bootstrap og CRM-settings. | Læs | Settings loader + Azure App Configuration | `AZURE_APPCONFIG_CONNECTIONSTRING` og miljøkilder | Alle modes | Systemforvaltede CRM-værdier er fjernet fra lokalfilen. |
| Failure notification | Valgfri fejlnotifikation. | Console fallback / evt. mail | `Gi.Batch.Shared` failure notification | `EnableFailureEmail`, `modtagereEmail`, evt. Azure.Email.* | Alle modes | Mail er sekundært i forhold til sletteworkflowet. |

## Næste anbefalede step

1. Kør lokal build/test på den rene `src`-erstatningsbranch.
2. Kør og dokumentér `VERIFYCRM`.
3. Kør særskilt `VERIFYCRM`, hvis der ønskes separat connectivity-evidens.
4. Kør kontrolleret `RUN`, hvis sletning er driftsmæssigt forsvarlig.
5. Opdatér statusmatrix og evidens efter første reelle kørsel.

## Testcases for eksterne kald

Særskilt testcase-dokument: `docs/jobs/testcases/dk.gi.app.konto.satser.slet.external-calls.md`
