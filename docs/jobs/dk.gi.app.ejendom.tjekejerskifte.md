# dk.gi.app.ejendom.tjekejerskifte

## Stamdata

- Job: `dk.gi.app.ejendom.tjekejerskifte`
- Iteration: dokumenteret DRYRUN-close-out-forberedelse med grøn teststatus og åben slutverificering
- Legacy-kilde: `GI-Batch/src/dk.gi.app.ejendom.tjekejerskifte`
- GI-kildekode brugt til analyse og portering: `HentEjendommeAktiveMedTinglystDatoRequest_V2.cs`
- Konverteringsretning: lokal Dataverse-læsning + lokal Service Bus-publicering for `TINGLYSNINGDATO`

## Kort beskrivelse

Legacy-jobbet finder aktive ejendomme med tinglysningsrelevant skødedato, springer ejendomme uden `ap_bfenummer` over og publicerer derefter queue-beskeder med label `TINGLYSNINGDATO`.

Den nye vej er modelleret som:
1. lokal App Configuration-bootstrap og startup-diagnostik
2. lokal Dataverse-læsning af kandidater fra `ap_ejendom`
3. lokal resolution af Service Bus-baseoplysninger via `config_configurationsetting`
4. payload-opbygning lokalt i jobprojektet
5. publicering til queue-sporet med planlagt forsinkelse pr. job

## Driftsprofil

- Modes: `VERIFYCRM`, `DRYRUN`, `RUN`
- Primær bootstrap: `AZURE_APPCONFIG_CONNECTIONSTRING`
- Lokal override: `appsettings.local.json`
- Lokal template: `appsettings.local.template.jsonc`
- Legacy schedule videreført fra `settings.job`

## Arkitekturstatus

- tynd `Program.cs`
- lokal composition root i `ServiceRegistry`
- application-lag for orkestrering
- infrastructure-adapters for Dataverse, config, messaging, runtime guard og failure notification
- legacy-filer lagt i `legacy-reference/` og ikke brugt som aktiv runtimevej

## Porteret forretningsadfærd i denne iteration

Følgende er eksplicit modelleret i startkonverteringen:
- læsning af aktive ejendomme fra `ap_ejendom`
- filtrering på `ap_tinglysningsidstkontrolleretdato` efter alder/null
- eksklusion af ejendomme markeret med `ap_undtagesforautomatiskskoededatotjek`
- spring over kandidater uden `ap_bfenummer`
- lokal payload-opbygning med `ap_ejendom`, kommunenummer, ejendomsnummer, BBR-nummer og BFE-felter
- publicering til Service Bus med label `TINGLYSNINGDATO`

## Kendt funktionel afgrænsning i denne iteration

Den nye læseforespørgsel er en direkte GI-fri startportering, men den er endnu ikke dokumenteret som fuld parity med hele legacy-managerens filtrering i `HentAktiveEjendommeMedSkoedeDatoFoer(...)`.

Det betyder, at jobbet er konverteret strukturelt og funktionelt i hovedsporet, men endnu ikke close-out-verificeret mod legacy-adfærden.

## Afhængighedsoversigt

### Direkte i ny vej
- `Microsoft.PowerPlatform.Dataverse.Client`
- `Microsoft.PowerPlatform.Dataverse.Client.Dynamics`
- `shared/Gi.Batch.Shared`

### Legacy beholdt som reference
- originale legacy-filer under `jobs/dk.gi.app.ejendom.tjekejerskifte/legacy-reference/`

## Config-/settingsoversigt

Kerneindstillinger i denne iteration:
- `Mode`
- `CrmConnectionTemplate`
- `CrmServerName`
- `CrmClientId`
- `CrmClientSecret`
- `CrmAuthority`
- `AuthorityMode` / `CrmAuthorityMode`
- `ServiceBusQueueName`
- `ServiceBusLabel`
- `ServiceBusSessionId`
- `ServiceBusBaseUrl`
- `ServiceBusSasKeyName`
- `ServiceBusSasKey`
- `QueueScheduleStepSeconds`
- `MaxDage`
- `MaxAntal`
- `modtagereEmail`

## Teststatus pr. kategori

### Unit
- `EjendomTjekEjerskiftePayloadFactoryTests`
- `EjendomTjekEjerskifteSettingsValidatorTests`

### Smoke
- `ServiceRegistrySmokeTests`

### Integration
- ikke oprettet i denne iteration

### Regression
- ikke oprettet i denne iteration

## Verificeret i denne iteration

- lokal `DRYRUN` på den nye vej er rapporteret bestået
- samlet build/test er rapporteret grøn lokalt efter seneste shared-fixrunde
- top-level GI runtime artifact verification er rapporteret genkørt og bestået uden fund af ekstra `dk.gi*.dll` i bin-output
- jobbet er derfor dokumenteret som GI-assembly-scan-bestået mellemtilstand med grøn teststatus, men ikke funktionelt slutverificeret

## Kendte testhuller

- ingen dokumenteret `VERIFYCRM`-evidens endnu
- ingen dokumenteret `RUN`-evidens endnu
- ingen dokumenteret parity-sammenligning mod legacy queryfilter endnu

## Autoritativ statusmatrix

| Felt | Status |
|---|---|
| GI NuGet-status | Ny lokal hovedvej etableret; endelig NuGet-close-out ikke særskilt dokumenteret i denne iteration |
| GI assembly-status | Top-level GI bin-scan rapporteret bestået uden fund af ekstra `dk.gi*.dll` |
| Moderniseringsstatus | Startkonverteret til ny Batchjobs-struktur |
| Afkoblingsstatus | Lokal Dataverse + Service Bus-hovedvej etableret |
| Clean/arkitekturstatus | God og testbar struktur med lokal payload- og queue-adapter |
| Shared-status | Shared-konsolideret for runtime/config/failure-komponenter; jobspecifik composition root forbliver lokal |
| Config-/deploystatus | `appsettings.local.json` og template medfølger |
| Build-/teststatus | Grøn build/test rapporteret lokalt; `DRYRUN` rapporteret bestået |
| Restpunkter og undtagelser | Mangler dokumenteret `VERIFYCRM`, `RUN`-evidens, dokumenteret queue-effekt og parity-verifikation |
| Verifikationsgrundlag | Kildeanalyse, portering, rapporteret grøn build/test, rapporteret `DRYRUN` og rapporteret GI bin-scan |

## Næste anbefalede step

1. Kør og dokumentér `VERIFYCRM` som separat forbindelsesevidens.
2. Kør kontrolleret `RUN` med lav `MaxAntal` mod kendt case.
3. Dokumentér faktisk queue-effekt og eventuel nedstrøms konsekvens.
4. Sammenlign kandidatpopulationen mod legacy-sporet før endelig close-out.

## Close-out-vurdering

Jobbet kan i denne iteration lukkes dokumentationsmæssigt som **mellem-close-out / close-out-forberedt**:
- den nye GI-frie hovedvej er etableret
- `DRYRUN` er rapporteret bestået
- GI bin-scan er rapporteret bestået
- men endelig slutverificering mangler fortsat, fordi `RUN`-effekt, `VERIFYCRM`, nedstrøms queue-konsekvens og parity mod legacy endnu ikke er dokumenteret

## Leveranceoversigt

- nyt job under `jobs/dk.gi.app.ejendom.tjekejerskifte/`
- nyt testprojekt under `jobs/dk.gi.app.ejendom.tjekejerskifte/dk.gi.app.ejendom.tjekejerskifte.Tests/`
- `legacy-reference/` med originale filer fra legacy-jobbet


## Supplerende evidens

- `docs/jobs/evidence/shared-konsolidering-og-teststatus-20260417.md`

## Eksterne kilder og integrationer

| Kilde | Formål | Adfærd | Teknisk adgangsvej | Settings / kilder | Modes | Risici / forbehold |
|---|---|---|---|---|---|---|
| Dataverse / CRM | Læser ejendomskandidater og evt. CRM-konfigurationsposter. | Læs | Lokal Dataverse-adapter + config resolution | CRM-settings fra App Configuration / environment | VERIFYCRM, DRYRUN, RUN | Parity mod legacy queryfilter er endnu ikke endeligt dokumenteret. |
| Service Bus | Publicerer `TINGLYSNINGDATO`-beskeder. | Publicér i RUN | Lokal Service Bus-sender | `ServiceBusQueueName`, `ServiceBusLabel`, evt. base URL/SAS fra systemkilder eller CRM config | RUN | DRYRUN må ikke publicere. |
| Azure App Configuration / environment | Leverer bootstrap og driftssettings. | Læs | JobConfigurationLoader + Azure App Configuration | `AZURE_APPCONFIG_CONNECTIONSTRING` og miljøkilder | Alle modes | Lokalfilen indeholder kun lokale overrides og obligatoriske standardnøgler. |

## Lokal settings-normalisering pr. 2026-04-20

- `appsettings.local.json` er normaliseret til batchjobs-standarden med obligatoriske lokale standardnøgler.
- systemforvaltede CRM- og Service Bus-kerneværdier er fjernet fra lokalfilen og forventes nu fra systemkilder.
- jobspecifikke lokale overrides er bevaret i det omfang de understøtter sikker lokal `DRYRUN`/diagnostik.

## Testcases for eksterne kald

Særskilt testcase-dokument: `docs/jobs/testcases/dk.gi.app.ejendom.tjekejerskifte.external-calls.md`

