# dk.gi.app.contact.selskab

## Stamdata

- Job: `dk.gi.app.contact.selskab`
- Iteration: næste søsterjob efter `dk.gi.app.contact.lassox.ophoer`
- Udgangspunkt: referencearkitektur fra kontakt-/CRM-familien i Batchjobs
- Legacy-kilde: `GI-Batch/src/dk.gi.app.contact.selskab`
- Legacy assembly/root namespace: `dk.gi.crm.app.contact.selskaber`
- GI-kildekode brugt til analyse og portering: `OpdaterSelskaberKDKmedAlleReeleEjereJaRequest_V2.cs`, `Kontakt-BLL.cs`, `ap_reelejerManager.cs`, `jobqueue.cs`, `AzureServiceBusQueueLabels.cs`

## Formål

Dette job finder selskaber hvor alle ultimative ejere står til KDK = ja, mens selve selskabet endnu ikke står til KDK = ja.

Den lokale implementation mapper legacy-flowet sådan:

1. find aktive `ap_reelejer` uden slutdato
2. join til selskabskontakt og filtrér til selskaber med CVR (`ap_virksomhedsid`) og KDK != ja
3. join til ultimativ ejer og læs ultimativ ejers KDK-status
4. gruppér på CVR og optæl antal ja/nej blandt ultimative ejere
5. kvalificér kun selskaber med mindst ét ja og nul nej
6. publicér `Mode=Contact`, `action=opdaterkdk`, `contactid=<selskab>` til `crmpluginjobs` / `KontoDiv`

## Arkitekturstatus

### Opnået i denne leverance
- Program-laget er simplificeret til tynd `Program.cs`
- lokal kontrakt mellem application og infrastructure er etableret
- startup-diagnostik og App Configuration-bootstrap er genbrugt fra referencearkitekturen
- legacy-requesten `OpdaterSelskaberKDKmedAlleReeleEjereJaRequest(Update=true)` er erstattet af lokal Dataverse-scan, lokal kvalificeringskerne og lokal Service Bus-publicering
- testprojekt er oprettet med Unit- og Smoke-spor
- legacy-reference er medtaget som `.original`-filer under `legacy-reference/`

### Bevidst afgrænsning i denne leverance
- CRM-effekt for dette job består i publicering af KDK-opdateringsjob; den efterfølgende opdatering udføres nedstrøms af modtagerjobbet
- jobbet er startet uden legacy-parity-spor; fokus er ny vej og faktisk execution-evidens
- faktisk nedstrøms opdatering af selskabsposterne i CRM er ikke en del af dette jobs egen RUN-evidens, men afhænger af modtagerjobbet på queue-sporet

## Konfigurationsmodel

Primær bootstrap-model er:

- environment variable: `AZURE_APPCONFIG_CONNECTIONSTRING`
- App Configuration / environment / App.config-relaterede kilder som source of truth
- `appsettings.local.json` kun som lokal override

Centrale settings i dette job:

- `Mode` (`VERIFYCRM`, `DRYRUN`, `RUN`)
- `CrmConnectionTemplate`
- `CrmServerName`
- `CrmClientId`
- `CrmClientSecret`
- `CrmAuthority`
- `CrmAuthorityMode`
- `ServiceBusBaseUrl`
- `ServiceBusSasKeyName`
- `ServiceBusSasKey`
- `ServiceBusQueueName` (default `crmpluginjobs`)
- `ServiceBusLabel` (default `KontoDiv`)
- `ServiceBusSessionId` (default `38852E10-DCF3-4453-A701-1A90AFDD1B14`)
- `QueueScheduleStepSeconds` (default `15`)
- `modtagereEmail`
- `TimeOutMinutter`
- `SecondsToSleep`
- `MaxWaitCount`

## Autoritativ statusmatrix

- GI NuGet-status: ny lokal vej anvendes i RUN; legacy request/manager/jobqueue er erstattet af lokale adapters for dette workflow
- GI assembly-status: verificeret via bin-scan; ingen ekstra `dk.gi*.dll` fundet i bin-output
- Moderniseringsstatus: gennemført for dette workflow
- Afkoblingsstatus: høj; ny lokal Dataverse-adapter, lokal kvalificeringskerne og lokal Service Bus-publicering anvendes i RUN
- Clean/arkitekturstatus: god og ensartet med referencearkitekturen
- Shared-status: shared-konsolideret for runtime/config/failure-komponenter
- Config-/deploystatus: appsettings og bootstrap-model er etableret og lokalt verificeret
- Build-/teststatus: grøn lokalt efter seneste shared-fixrunde
- Functional RUN-status: bestået på ny vej
- Restpunkter og undtagelser:
  - faktisk nedstrøms CRM-opdatering ligger i modtagerjobbet og er derfor ikke en direkte del af dette jobs egen write-evidens
  - eventuel ekstra forretningseffekt-verifikation kan tilføjes senere som særskilt evidensspor, hvis ønsket
- Verifikationsgrundlag: lokal grøn build/test, lokal RUN-log med queue-effekt, bin-scan uden ekstra `dk.gi*.dll`, legacy-kode, GI-Nugets kildekode og referencearkitektur

## Testspor i leverancen

- Unit:
  - kvalificeringslogik for selskaber med alle ultimative ejere = ja
  - payload til `opdaterkdk`
  - settings-validator
  - publisher-baseline uden kandidater
- Smoke:
  - service registry i `DRYRUN` uden Config Store


## Seneste RUN-verifikation

Lokal RUN på den nye vej blev rapporteret bestået med følgende centrale observationer:

- startup-diagnostik gennemført med App Configuration-bootstrap
- Dataverse klient oprettet succesfuldt
- læsning af ejerrelationer gennemført på 3 sider med samlet `ObserveredeEjerrelationer=1146`
- Service Bus-settings resolved via CRM `config_configurationsetting`
- 2 konkrete KDK-opdateringsjob publiceret til `crmpluginjobs` / `KontoDiv`
- proces afsluttet med workflow-resumé og uden fallback til legacy

Dokumenteret resultat:

- `ObserveredeEjerrelationer=1146`
- `Kandidater=2`
- `Publiceret=2`
- `Mode=RUN`
- publicerede selskaber:
  - `f0ad6205-73bc-e211-820e-005056843758` (`25970225`)
  - `981270f6-583e-e911-8103-00505602060a` (`36560193`)

## Næste anbefalede step

1. bevar jobbet som reference for næste fan-out-job i samme CRM-/kontaktmønster
2. tilføj evt. særskilt evidens for nedstrøms CRM-opdatering, hvis dette ønskes som ekstra forretningseffektspor
3. normalisér evt. fælles mønstre yderligere i shared, hvis samme struktur genbruges på næste job

## Leveranceoversigt

- nyt job under `jobs/dk.gi.app.contact.selskab/`
- nyt testprojekt under `jobs/dk.gi.app.contact.selskab/dk.gi.app.contact.selskab.Tests/`
- `legacy-reference/` med originale jobfiler
- opdateret `GI.Batch.sln`
- opdateret `README.md`
- opdateret `docs/PROJEKTBESKRIVELSE.txt`


## Evidens

- `docs/jobs/evidence/dk.gi.app.contact.selskab/contact-selskab-run-closeout-20260417.md`
- `docs/jobs/evidence/root-bin-gi-assembly-scan-20260417.md`

## Slutstatus

**Jobbet er teknisk close-out-klar på den nye vej.**

Det betyder i denne leverance:
- grøn build/test er rapporteret lokalt
- RUN på ny vej er bestået
- faktisk queue-/CRM-effekt er dokumenteret (`Publiceret=2`)
- bin-scan fandt ingen ekstra `dk.gi*.dll`

Jobbet beskrives ikke som særskilt management-closeout, men som teknisk close-out-klar reference for næste fan-out.

## Eksterne kilder og integrationer

| Kilde | Formål | Adfærd | Teknisk adgangsvej | Settings / kilder | Modes | Risici / forbehold |
|---|---|---|---|---|---|---|
| Dataverse / CRM | Læser ejerrelationer, ultimative ejere og konfigurationsposter. | Læs | Lokal Dataverse-adapter + config resolution | CRM-settings fra App Configuration / environment | VERIFYCRM, DRYRUN, RUN | Kandidatpopulation og parity kan fortsat udbygges som særskilt spor. |
| Service Bus | Publicerer `KontoDiv`-beskeder med `action=opdaterkdk`. | Publicér i RUN | Lokal Service Bus-sender | `ServiceBusQueueName`, `ServiceBusLabel`, `ServiceBusSessionId`, evt. base URL/SAS fra systemkilder eller CRM config | RUN | DRYRUN må ikke publicere. |
| Azure App Configuration / environment | Leverer bootstrap og driftssettings. | Læs | JobConfigurationLoader + Azure App Configuration | `AZURE_APPCONFIG_CONNECTIONSTRING` og miljøkilder | Alle modes | Systemforvaltede queue-/CRM-værdier holdes ude af lokalfilen. |

## Lokal settings-normalisering pr. 2026-04-20

- `appsettings.local.json` er normaliseret til batchjobs-standarden med obligatoriske lokale standardnøgler.
- systemforvaltede CRM- og Service Bus-kerneværdier er fjernet fra lokalfilen og forventes nu fra systemkilder.
- jobspecifikke lokale overrides er bevaret i det omfang de understøtter sikker lokal `DRYRUN`/diagnostik.

## Testcases for eksterne kald

Særskilt testcase-dokument: `docs/jobs/testcases/dk.gi.app.contact.selskab.external-calls.md`

