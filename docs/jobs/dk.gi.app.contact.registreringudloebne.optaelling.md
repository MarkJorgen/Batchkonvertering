# dk.gi.app.contact.registreringudloebne.optaelling

## Stamdata

- Job: `dk.gi.app.contact.registreringudloebne.optaelling`
- Iteration: næste fase efter pilot + fase 0, nu close-out’et på management-niveau
- Udgangspunkt: referencearkitektur fra `dk.gi.app.contact.registrering.optaelling`
- Legacy-kilde: `GI-Batch/src/dk.gi.app.contact.registreringudloebne.optaelling`
- GI-kildekode brugt til analyse: `ContactBLL-Registreringer.cs`, `ContactBllRegisteringResponse.cs`, `RegistreringManager.V2.cs`, `AzureServiceBusQueueLabels.cs`

## Formål

Dette job er fan-out af pilotmønstret til søsterjobbet for udløbne/bortfaldne registreringer.

Den lokale implementation mapper legacy-flowet sådan:

1. find udløbne/bortfaldende registreringer
2. find afsluttede treklip
3. publicér Azure Service Bus-job til `crmpluginjobs` med label `laan`, når der faktisk findes kandidater med afsluttede treklip
4. payload `mode=laan`, `action=updudloebet`, `sagsnr`, `id`

## Arkitekturstatus

### Opnået i denne iteration
- Program-laget er simplificeret til tynd `Program.cs`
- lokal kontrakt mellem application og infrastructure er etableret
- startup-diagnostik er udvidet til App Configuration-bootstrap via `AZURE_APPCONFIG_CONNECTIONSTRING`
- `ConfigStoreConnectionString` er nedgraderet til legacy-kompatibilitet og bruges ikke som primær model
- GI-kaldet til `ContactBLL.FindAndCreateJobsForContactWithRonoutRegistrering` er erstattet af lokal Dataverse-scan + lokal Service Bus-publicering
- Service Bus base/SAS kan resolves sent via CRM `config_configurationsetting`
- hård startup-validering af Service Bus er fjernet, så jobbet ikke stopper før execution, når publicering måske slet ikke bliver nødvendig
- testprojektet er ryddet op, og test-output bruger kort `artifacts\obj`/`artifacts\bin`-sti for at undgå path-length-fejl i design-time build
- grøn build/test er rapporteret i lokal validering
- GI runtime artifact verification er genkørt på succesfuld build og rapporteret bestået uden `dk.gi*.dll`
- root-scan mod samtlige projekters `bin`-output fandt ligeledes ingen ekstra `dk.gi*.dll`

### Accepterede undtagelser ved close-out
- funktionel parity mod legacy-referencevejen er ikke bevist
- legacy comparison-sporet er ikke fuldt sammenligningsklart
- faktisk CRM-forretningseffekt er ikke fuldt dokumenteret ud over den konkrete RUN-evidens for `Scannet=379` og `Publiceret=0`

## Management close-out beslutning

Dette job er lukket som **full end-state close-out med accepterede undtagelser** på management-niveau.

Det er en bevidst styringsmæssig override af projektets normale regel om, at parity mod legacy og fuld forretningseffekt normalt skal være dokumenteret før ærlig teknisk slutlukning.

Det der ligger til grund for close-out-beslutningen er:
- grøn build/test
- bestået RUN på ny vej
- gennemført GI runtime artifact verification på succesfuld build
- konsolideret config-/deploymodel
- opdateret dokumentation og statusmatrix

## Faktisk RUN-evidens i denne iteration

Lokal RUN er rapporteret bestået med følgende centrale observationer:

- startup-diagnostik viste CRM-settings som `FOUND`
- Dataverse-klienten blev oprettet korrekt
- Service Bus blev resolved via `crm config_configurationsetting`
- workflowet blev gennemført uden exception
- resultatet for den konkrete kørsel var:
  - `Scannet=379`
  - `Publiceret=0`
  - `Ingen afsluttede treklip fundet`

Det er dermed et teknisk bestået RUN-spor på den nye vej.

## GI runtime artifact verification

GI runtime artifact verification blev genkørt mod den succesfulde build med runtime output:

`C:\Users\MPAGI\source\repos\Batchjobs\jobs\dk.gi.app.contact.registreringudloebne.optaelling\dk.gi.app.contact.registreringudloebne.optaelling\bin\Debug\net48`

Rapporteret resultat:
- markdown report skrevet under `docs/jobs/evidence/dk.gi.app.contact.registreringudloebne.optaelling/`
- JSON report skrevet under `docs/jobs/evidence/dk.gi.app.contact.registreringudloebne.optaelling/`
- endelig status: `GI runtime artifact verification bestået: ingen dk.gi artifacts fundet.`

Det betyder, at den aktuelle succesfulde build er dokumenteret uden ekstra `dk.gi*.dll` i runtime output.

## Konfigurationsmodel efter denne iteration

Primær bootstrap-model er nu:

- environment variable: `AZURE_APPCONFIG_CONNECTIONSTRING`
- App Configuration / environment / App.config-relaterede kilder som source of truth
- `appsettings.local.json` kun som lokal override

For dette job gælder desuden:

- `ServiceBusBaseUrl`, `ServiceBusSasKeyName` og `ServiceBusSasKey` kan resolves via CRM `config_configurationsetting`, hvis de ikke kommer fra lokal/App Configuration
- `ServiceBusQueueName`, `ServiceBusLabel` og `ServiceBusSessionId` holdes ikke længere kunstigt i live af kode-defaults
- hvis publicering faktisk bliver nødvendig, og de tre message-settings mangler, fejler publiceringen dér med præcis fejltekst

## Autoritativ statusmatrix

- GI NuGet-status: lukket for denne leverance
- GI assembly-status: verificeret på succesfuld build og suppleret af root bin-scan uden ekstra `dk.gi*.dll`
- Moderniseringsstatus: lukket for denne leverance
- Afkoblingsstatus: lukket med accepterede undtagelser
- Clean/arkitekturstatus: acceptabel og styringsmæssigt lukket
- Shared-status: shared-konsolideret for runtime/config/failure-komponenter i nuværende mønsterfamilie
- Config-/deploystatus: lukket
- Build-/teststatus: grøn
- Functional RUN-status: bestået på ny vej
- Comparison legacy baseline-status: ikke fuldt sammenligningsklar, accepteret som undtagelse
- Restpunkter og undtagelser:
  - funktionel parity mod legacy er ikke bevist
  - faktisk CRM-forretningseffekt er ikke fuldt dokumenteret
  - comparison-sporet er fortsat ikke fuldt sammenligningsklart
- Verifikationsgrundlag: grøn build/test, lokal RUN-log rapporteret i chatten, runtime artifact verification rapporteret bestået, referencearkitektur, legacy-kode og GI-Nugets kildekode

## Testspor i leverancen

- Unit:
  - validator tillader nu RUN uden hård startup-Service Bus-validering
  - sender fejler kontrolleret, hvis message-settings mangler på publiceringstidspunktet
  - publisher bevarer legacy-adfærd, når der ikke findes afsluttede treklip
  - connection string-/settings parsing
- Smoke:
  - service registry / bootstrap
- Regression:
  - Config Store bootstrap via `AZURE_APPCONFIG_CONNECTIONSTRING`
  - lokal JSON må ikke overskrive miljø med tomme værdier
  - kort test-outputsti for at undgå `GeneratedMSBuildEditorConfig.editorconfig`-path-fejl
  - robust GI runtime artifact verification script uden falsk positiv på job-`exe`

## Evidens

- `docs/jobs/evidence/dk.gi.app.contact.registreringudloebne.optaelling-run-success.md`
- `docs/jobs/evidence/dk.gi.app.contact.registreringudloebne.optaelling-runtime-gi-verification.md`
- `docs/jobs/evidence/root-bin-gi-assembly-scan-20260417.md`
- lokal genereret JSON/MD artifact verification rapport under `docs/jobs/evidence/dk.gi.app.contact.registreringudloebne.optaelling/`

## Slutstatus

**Jobbet er lukket som full end-state close-out med accepterede undtagelser.**

Det er den autoritative status for denne leverance. De åbne undtagelser videreføres ikke som blokering for close-out, men som eventuelle senere evidens- eller forbedringsspor.

## Eksterne kilder og integrationer

| Kilde | Formål | Adfærd | Teknisk adgangsvej | Settings / kilder | Modes | Risici / forbehold |
|---|---|---|---|---|---|---|
| Dataverse / CRM | Læser kandidater for udløbne registreringer og evt. CRM-konfigurationsposter. | Læs | Lokal Dataverse-adapter + config resolution | CRM-settings fra App Configuration / environment | VERIFYCRM, DRYRUN, RUN | Sen Service Bus-resolution betyder, at ikke alle queue-settings valideres ved startup. |
| Service Bus | Publicerer beskeder for udløbne registreringer. | Publicér i RUN | Lokal Service Bus-sender | `ServiceBusQueueName`, `ServiceBusLabel`, evt. base URL/SAS via CRM config eller systemkilder | RUN | DRYRUN må ikke publicere. |
| Azure App Configuration / environment | Leverer bootstrap og primære driftssettings. | Læs | JobConfigurationLoader + Azure App Configuration | `AZURE_APPCONFIG_CONNECTIONSTRING` og miljøkilder | Alle modes | Lokalfil er kun override og må ikke være source of truth. |

## Lokal settings-normalisering pr. 2026-04-20

- `appsettings.local.json` er normaliseret til batchjobs-standarden med obligatoriske lokale standardnøgler.
- systemforvaltede CRM- og Service Bus-kerneværdier er fjernet fra lokalfilen og forventes nu fra systemkilder.
- jobspecifikke lokale overrides er bevaret i det omfang de understøtter sikker lokal `DRYRUN`/diagnostik.

## Testcases for eksterne kald

Særskilt testcase-dokument: `docs/jobs/testcases/dk.gi.app.contact.registreringudloebne.optaelling.external-calls.md`

