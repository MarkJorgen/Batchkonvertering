# dk.gi.app.konto.startarealtjek

## Stamdata

- Job: `dk.gi.app.konto.startarealtjek`
- Iteration: close-out-opdateret efter bestået RUN, grøn build/test og ren GI bin-scan
- Legacy-kilde: `GI-Batch/src/dk.gi.app.konto.startarealtjek`
- Legacy assembly/root namespace: `dk.gi.crm.app.konto.startarealtjek`
- GI-kildekode brugt til analyse og portering: `FindOgOpdaterKontoArealCheckRequest.cs`, `Ap_KontoManager.V2.cs`, `IncidentManager.cs`, option set-definitioner for `AP_konto`

## Kort beskrivelse

Dette job finder konti, der skal have flaget `ap_emneforarealtjek` sat eller fjernet ud fra arealtjekreglerne, og publicerer derefter et begrænset antal kørselskandidater til queue-sporet for videre behandling.

Den nye vej er modelleret som:
1. lokal App Configuration-bootstrap og startup-diagnostik
2. lokal Dataverse-læsning af batchsettings, konti, statuskode 22 og åbne sager
3. lokal vurdering af om en konto fortsat skal være emne for arealtjek
4. opdatering af `ap_emneforarealtjek` i RUN-mode
5. lokal udvælgelse af batchkandidater pr. ejendomstype
6. publicering af `AREALCHECKSAG`-beskeder til `crmpluginjobs` / `ArealTjekKonto`

## Driftsprofil

- Modes: `VERIFYCRM`, `DRYRUN`, `RUN`
- Primær bootstrap: `AZURE_APPCONFIG_CONNECTIONSTRING`
- Lokal override: `appsettings.local.json`
- Legacy schedule videreført fra `settings.job`: `0 10 06 * * 1-6`

## Arkitekturstatus

- tynd `Program.cs`
- lokal composition root i `ServiceRegistry`
- application-lag for orkestrering og udvælgelseslogik
- infrastructure-adapters for Dataverse, config, queue og failure notification
- write-fase og queue-fase er diagnostisk synlige i RUN-mode
- legacy-filer lagt i `legacy-reference/` og ikke brugt som aktiv runtimevej

## Porteret forretningsadfærd i denne iteration

Følgende er eksplicit modelleret i startkonverteringen:
- læsning af `app.konto.arealtjek.*` fra CRM `config_configurationsetting`
- filtrering af konti efter aktiv status, ejendomstype, lovgrundlag, bindingstype, småhus, flere ibrugtagningsår og sidste arealtjek
- eksklusion af konti med statuskode 22 (`Arealdifference`)
- eksklusion af konti med åbne sager, bortset fra emner der tillader kontoselvbetjening
- batchudvælgelse pr. ejendomstype med ældste/null `ap_arealtjekgennemfoert` først
- publicering af payload med `action=AREALCHECKSAG`, `Kilde=Batch:dk.gi.app.konto.startarealtjek`, `id` og `Kontonr`

## Dokumenteret RUN-resultat

Lokal RUN på den nye vej blev rapporteret bestået med følgende observerede resultat:

- App Configuration-bootstrap og startup-diagnostik gennemført succesfuldt
- Dataverse klient oprettet succesfuldt
- kontolæsning og kvalificeringsflow gennemført for hele populationen
- write-fasen gennemført med opdatering af `ap_emneforarealtjek` på `210` konto(er)
- Service Bus-publicering gennemført med `1` publiceret arealtjek-job
- workflow afsluttet med samlet resumé uden fejl

Dokumenteret summary fra RUN:

- `Scannet=5377`
- `Emner=4800`
- `IkkeEmner=577`
- `Publiceret=1`
- `Mode=RUN`

## Afhængighedsoversigt

### Direkte i ny vej
- `Microsoft.PowerPlatform.Dataverse.Client`
- `Microsoft.PowerPlatform.Dataverse.Client.Dynamics`
- `shared/Gi.Batch.Shared`

### Legacy beholdt som reference
- originale legacy-filer under `jobs/dk.gi.app.konto.startarealtjek/legacy-reference/`
- ingen påstand i denne iteration om fuld GI-fri runtimeverifikation endnu

## Config-/settingsoversigt

Kerneindstillinger i denne iteration:
- `Mode`
- `CrmConnectionTemplate`
- `CrmServerName`
- `CrmClientId`
- `CrmClientSecret`
- `CrmAuthority`
- `CrmAuthorityMode`
- `ServiceBusQueueName`
- `ServiceBusLabel`
- `ServiceBusSessionId`
- `ServiceBusBaseUrl`
- `ServiceBusSasKeyName`
- `ServiceBusSasKey`
- `app.konto.arealtjek.antalaar`
- `BuildYearBefore`
- `app.konto.arealtjek.antalprbatch.au`
- `app.konto.arealtjek.antalprbatch.ef`
- `app.konto.arealtjek.antalprbatch.ab`
- mail-/failure-spor fra den etablerede Batchjobs-standard

## Teststatus pr. kategori

### Unit
- `KontoStartArealTjekSelectionEngineTests`
- `KontoStartArealTjekPayloadFactoryTests`
- `KontoStartArealTjekSettingsValidatorTests`
- `KontoStartArealTjekJobPublisherTests`

### Smoke
- `ServiceRegistrySmokeTests`

### Integration
- ikke oprettet i denne iteration

### Regression
- endnu ikke særskilt kategoriudbygget i denne iteration

## Kendte testhuller

- ingen egentlig Dataverse integrationstest endnu
- ingen særskilt VERIFYCRM-evidens endnu
- ingen batchvis Dataverse write-optimering; RUN bruger fortsat enkeltvise konto-opdateringer i write-fasen

## Autoritativ statusmatrix

| Felt | Status |
|---|---|
| GI NuGet-status | Ny lokal hovedvej etableret og teknisk close-out-klar; ingen særskilt fuld NuGet-matrix skrevet endnu |
| GI assembly-status | Bestået: top-level bin-scan fandt ingen ekstra `dk.gi*.dll` |
| Moderniseringsstatus | Konverteret til ny Batchjobs-målarkitektur og RUN-verificeret |
| Afkoblingsstatus | Ny lokal Dataverse + Service Bus-hovedvej bevist i RUN-mode |
| Clean/arkitekturstatus | God og testbar struktur; write-/queue-faser synlige i diagnostik |
| Shared-status | Shared-konsolideret for runtime/config/failure-komponenter; jobspecifik composition root forbliver lokal |
| Config-/deploystatus | `appsettings.local.json` og `appsettings.local.template.jsonc` medfølger; App Configuration-bootstrap verificeret i RUN |
| Build-/teststatus | Grøn build/test rapporteret lokalt efter seneste shared-fixrunde |
| Restpunkter og undtagelser | VERIFYCRM-evidens ikke særskilt dokumenteret; write-fasen er funktionelt korrekt men endnu ikke batchoptimeret |
| Verifikationsgrundlag | Grøn build/test, bestået RUN, dokumenteret write-/queue-effekt og root bin-scan |

## Næste anbefalede step

1. Behandl `dk.gi.app.konto.startarealtjek` som teknisk close-out-klar på den nye vej.
2. Bevar write-fasen som senere optimeringskandidat, hvis runtime skal nedbringes yderligere.
3. Brug jobdokument, RUN-evidens og root bin-scan som referenceformat før næste job.
4. Start næste fan-out-job efter samme mønster med App Configuration-bootstrap, diagnostik, testprojekt og separat close-out-evidens.

## Leveranceoversigt

- nyt job under `jobs/dk.gi.app.konto.startarealtjek/`
- nyt testprojekt under `jobs/dk.gi.app.konto.startarealtjek/dk.gi.app.konto.startarealtjek.Tests/`
- `legacy-reference/` med originale filer fra legacy-jobbet
- start-evidens og RUN-closeout-evidens under `docs/jobs/evidence/dk.gi.app.konto.startarealtjek/`

## Eksterne kilder og integrationer

| Kilde | Formål | Adfærd | Teknisk adgangsvej | Settings / kilder | Modes | Risici / forbehold |
|---|---|---|---|---|---|---|
| Dataverse / CRM | Læser batchsettings, konti og åbne sager og opdaterer `ap_emneforarealtjek` i RUN. | Læs i DRYRUN, læs + skriv i RUN | Lokal Dataverse-adapter + config resolution | CRM-settings fra App Configuration / environment | VERIFYCRM, DRYRUN, RUN | RUN-evidens foreligger, men integrationstests kan stadig udbygges. |
| Service Bus | Publicerer `AREALCHECKSAG`-beskeder. | Publicér i RUN | Lokal Service Bus-sender | `ServiceBusQueueName`, `ServiceBusLabel`, `ServiceBusSessionId`, evt. base URL/SAS fra systemkilder | RUN | DRYRUN må ikke publicere. |
| Azure App Configuration / environment | Leverer bootstrap og driftssettings. | Læs | JobConfigurationLoader + Azure App Configuration | `AZURE_APPCONFIG_CONNECTIONSTRING` og miljøkilder | Alle modes | Systemforvaltede queue-/CRM-værdier holdes ude af lokalfilen. |

## Lokal settings-normalisering pr. 2026-04-20

- `appsettings.local.json` er normaliseret til batchjobs-standarden med obligatoriske lokale standardnøgler.
- systemforvaltede CRM- og Service Bus-kerneværdier er fjernet fra lokalfilen og forventes nu fra systemkilder.
- jobspecifikke lokale overrides er bevaret i det omfang de understøtter sikker lokal `DRYRUN`/diagnostik.

## Testcases for eksterne kald

Særskilt testcase-dokument: `docs/jobs/testcases/dk.gi.app.konto.startarealtjek.external-calls.md`

