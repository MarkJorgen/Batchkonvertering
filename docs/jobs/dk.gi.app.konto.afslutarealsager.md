# dk.gi.app.konto.afslutarealsager

## Stamdata

- Job: `dk.gi.app.konto.afslutarealsager`
- Iteration: fase 7 discovery + forcekontonr + digital-post-staging + areal-carry-forward + AREALSUM2KONTO-seam til Batchjobs-målarkitektur, nu med grøn teststatus efter shared-fixrunde
- Legacy-kilde: `GI-Batch/src/dk.gi.app.konto.afslutarealsager`
- Legacy assembly/root namespace: `dk.gi.crm.app.konto.afslutarealsager`
- GI-kildekode brugt til analyse: `AfslutArealSagerRequest.cs`, `pdf.cs`, `HentSagerRequest_V2.cs`, `OpretOpgaveRequest_V2.cs`, `UploadFilRequest_V2.cs`, `LukAktivitetRequest_V2.cs`, `LukArealCheckSmaaDifRequest.cs`

## Kort beskrivelse

Dette job finder åbne konto-/areal-sager for den angivne bruger, opløser relateret konto, ejendom og primær kontakt i Dataverse og genererer herefter det afsluttende brev på den nye vej.

Den aktuelle leverance er nu **fase 7**. Det betyder, at følgende kan frigives når `Mode=RUN` og `AllowPartialRun=true`:

1. oprettelse af aktivitet i CRM
2. upload af PDF som note til aktiviteten
3. lukning af aktiviteten
4. **valgfri** publicering af closeout-job til Service Bus (`KontoDiv`) når `EnableCloseoutQueueRun=true`
5. **valgfri** direkte incident-closeout i Dataverse når `EnableDirectIncidentCloseoutRun=true`
6. **valgfrit** lokalt areal carry-forward-seam når `EnableCarryForwardArealRun=true`
7. **valgfri** sletning af 0-regnskab når `EnableDeleteZeroRegnskabRun=true`
8. **valgfri** publicering af `AREALSUM2KONTO` når `EnableArealSumQueueRun=true`
9. **valgfri** discovery-run uden owner-filter når `EnableDiscoveryRun=true`

Følgende er fortsat **ikke** frigivet i den nye vej:

1. afsendelse til Digital Post
2. lokal GI-ækvivalent arealberegning/arealopdatering
3. endelig verifikation af forretningseffekt i den nye vej

## Driftsprofil

- Modes: `VERIFYCRM`, `DRYRUN`, `RUN`
- `RUN` uden `AllowPartialRun=true` er fortsat blokeret
- `RUN` med `AllowPartialRun=true` frigiver lokal write-vej (aktivitet + PDF-note + lukning af aktivitet)
- `EnableCloseoutQueueRun=true` udvider partial run med publicering af `luksagaktiviteter` til Service Bus `KontoDiv`
- `EnableDirectIncidentCloseoutRun=true` udvider partial run med direkte incident-closeout i Dataverse
- `ForceIncidentId`, `ForceSagsnummer` og `ForceKontonr` kan bruges som smalle override-filtre ved senere teknisk verificering
- `EnableDiscoveryRun=true` giver en read-only kandidatliste uden owner-filter; `DiscoveryLimit` styrer hvor mange der logges
- Primær bootstrap: `AZURE_APPCONFIG_CONNECTIONSTRING`
- Lokal override: `appsettings.local.json`
- Legacy schedule videreført fra `settings.job`

## Arkitekturstatus

- tynd `Program.cs`
- lokal composition root i `ServiceRegistry`
- application-lag for orkestrering og brevforberedelse
- infrastructure-adapter for Dataverse-read/write
- separat dokumentgenerator med Aspose-skabelon fra legacy-jobbet
- lokal Service Bus-closeout publisher som nyt seam
- lokal direct incident-closeout som eksplicit fase-4-seam
- lokalt areal carry-forward-seam med valgfri `AREALSUM2KONTO`-publicering
- legacy-filer lagt i `legacy-reference/` og ikke brugt som aktiv runtimevej

## Porteret adfærd i denne iteration

Følgende er nu eksplicit modelleret i fase 6:
- App Configuration-bootstrap og startup-diagnostik
- lokal Dataverse-connect via `Microsoft.PowerPlatform.Dataverse.Client`
- opslag af systembruger for `BrugerArealSager`
- opslag af emnet `Konto / Arealændring`
- læsning af åbne sager med `followupby <= now + OpfoelgesFraPlusDage`
- valgfri smalle override-filtre (`ForceIncidentId`, `ForceSagsnummer`, `ForceKontonr`)
- read-only discovery-mode uden owner-filter med logning af kandidater (incidentid, sagsnr, kontonr, owner, subject, followup)
- nul-hit diagnostik for owner/subject/followup-kæden når standardscan giver 0 incidents
- opløsning af relateret konto, ejendom og primær kontakt via connection-sporet
- opbygning af brev-flettedata og PDF-generering via eksisterende skabelon
- oprettelse af CRM-aktivitet i partial run
- upload af PDF som note til aktivitet i partial run
- lukning af aktivitet i partial run
- resolution af Service Bus-settings via `config_configurationsetting`
- lokal payload/publisher for `Mode=Incident`, `action=luksagaktiviteter`, `beskrivelse=Luk areal check`
- direkte incident-closeout via Dataverse bag eksplicit toggle
- lokalt carry-forward af åbent `ap_areal` ved brug af seneste regnskabsdato
- valgfri sletning af 0-regnskab (`ap_regnskab` med årsagskode `02`)
- lokal payload/publisher for `AREALSUM2KONTO`
- suppression af failure-notification i `DRYRUN` og `VERIFYCRM`

## Bevidst ikke frigivet endnu

- Digital Post er ikke porteret endnu
- GI-specialregler for arealberegning er ikke porteret endnu; fase 7 bruger discovery-mode, et lokalt carry-forward-seam og digital-post-staging, ikke fuld GI-paritet
- direct incident-closeout og areal carry-forward er tekniske fase-5-veje og ikke dokumenteret som fuld GI-ækvivalent closeout
- denne leverance skal derfor stadig beskrives som **mellemtilstand**, ikke close-out

## Afhængighedsoversigt

### Direkte i ny vej
- `Microsoft.PowerPlatform.Dataverse.Client`
- `Microsoft.PowerPlatform.Dataverse.Client.Dynamics`
- `Aspose.Words`
- `shared/Gi.Batch.Shared`

### Legacy beholdt som reference
- originale legacy-filer under `jobs/dk.gi.app.konto.afslutarealsager/legacy-reference/`
- originale Aspose-ressourcer og brevskabelon genbrugt i ny vej
- GI-kildekode analyseret for `LukArealCheckSmaaDifRequest`

## Config-/settingsoversigt

Kerneindstillinger i denne iteration:
- `Mode`
- `CrmAuthorityMode`
- `BrugerArealSager`
- `OpfoelgesFraPlusDage`
- `TilladSendTilDigitalPost`
- `AllowPartialRun`
- `EnableCloseoutQueueRun`
- `EnableDirectIncidentCloseoutRun`
- `EnableCarryForwardArealRun`
- `EnableDeleteZeroRegnskabRun`
- `EnableArealSumQueueRun`
- `DirectIncidentCloseStatusCode`
- `ForceIncidentId`
- `ForceSagsnummer`
- `ForceKontonr`
- `ServiceBusQueueName` (default `crmpluginjobs`)
- `ServiceBusLabel` (default `KontoDiv`)
- CRM-bootstrap kommer fortsat primært fra App Configuration / environment
- lokal `appsettings.local.json` er fortsat slanket til lokale overrides

## Konkrete settings-kombinationer for dette job

Nedenstående kombinationer er **job-specifikke** for `dk.gi.app.konto.afslutarealsager` og supplerer den generelle projektregel om, at App Configuration / environment er source of truth, mens `appsettings.local.json` kun er lokal override.

### 1. Minimal lokal override
Bruges som udgangspunkt i lokal udvikling, når de egentlige CRM-/Config Store-værdier kommer fra environment/App Configuration.

```json
{
  "Mode": "DRYRUN",
  "CrmAuthorityMode": "AsConfigured",
  "EnableLocalDebugLogging": false,
  "LocalDebugLogPath": "C:\\Temp\\GI.Batch\\LocalLogs",
  "BrugerArealSager": "",
  "OpfoelgesFraPlusDage": 0,
  "TilladSendTilDigitalPost": false,
  "AllowPartialRun": false
}
```

### 2. VERIFYCRM
Bruges til ren forbindelsesvalidering uden workflowkørsel. `BrugerArealSager` er ikke obligatorisk i denne mode.

```json
{
  "Mode": "VERIFYCRM"
}
```

### 3. Discovery-run uden kendt kontonr
Bruges når der ikke findes et kendt `kontonr`, `incidentid` eller `sagsnr`. Discovery er read-only og logger kandidater uden owner-filter.

```json
{
  "Mode": "DRYRUN",
  "EnableDiscoveryRun": true,
  "DiscoveryLimit": 20,
  "AllowPartialRun": false
}
```

### 4. Standard DRYRUN for ejer-/emnefilteret
Bruges til teknisk verificering af den normale query-vej uden writes.

```json
{
  "Mode": "DRYRUN",
  "BrugerArealSager": "sma@gi.dk",
  "OpfoelgesFraPlusDage": 0,
  "AllowPartialRun": false
}
```

### 5. Målrettet DRYRUN med smal selector
Bruges når en konkret sag, et konkret sagsnummer eller et konkret kontonr er kendt. Kun ét af felterne bør normalt sættes ad gangen.

```json
{
  "Mode": "DRYRUN",
  "BrugerArealSager": "sma@gi.dk",
  "ForceIncidentId": "<incident-guid>"
}
```

Alternativt:

```json
{
  "Mode": "DRYRUN",
  "BrugerArealSager": "sma@gi.dk",
  "ForceSagsnummer": "SAG-12345"
}
```

eller:

```json
{
  "Mode": "DRYRUN",
  "BrugerArealSager": "sma@gi.dk",
  "ForceKontonr": "45-06362"
}
```

### 6. Partial RUN basis
Frigiver den lokale write-vej for aktivitet, PDF-note og aktivitetslukning. Dette er stadig mellemtilstand og ikke fuld close-out.

```json
{
  "Mode": "RUN",
  "BrugerArealSager": "sma@gi.dk",
  "AllowPartialRun": true
}
```

### 7. Partial RUN med closeout-queue
Udvider partial run med publicering af closeout-job til Service Bus (`KontoDiv`).

```json
{
  "Mode": "RUN",
  "BrugerArealSager": "sma@gi.dk",
  "AllowPartialRun": true,
  "EnableCloseoutQueueRun": true
}
```

### 8. Partial RUN med direkte incident-closeout
Udvider partial run med direkte lukning af incident i Dataverse.

```json
{
  "Mode": "RUN",
  "BrugerArealSager": "sma@gi.dk",
  "AllowPartialRun": true,
  "EnableDirectIncidentCloseoutRun": true,
  "DirectIncidentCloseStatusCode": 5
}
```

### 9. Partial RUN med areal carry-forward og AREALSUM2KONTO
Bruges til teknisk verificering af fase-5-seamet for nyt `ap_areal`, sletning af 0-regnskab og publicering af `AREALSUM2KONTO`.

```json
{
  "Mode": "RUN",
  "BrugerArealSager": "sma@gi.dk",
  "AllowPartialRun": true,
  "EnableCarryForwardArealRun": true,
  "EnableDeleteZeroRegnskabRun": true,
  "EnableArealSumQueueRun": true
}
```

### 10. Digital post som stub/staging
Bruges kun til lokal teknisk verificering af digital-post-sporet. Der sendes ikke ekstern digital post; der stages kun som note.

```json
{
  "Mode": "DRYRUN",
  "BrugerArealSager": "sma@gi.dk",
  "TilladSendTilDigitalPost": true,
  "EnableDigitalPostStubRun": true
}
```

### 11. Discovery som forberedelse til senere målrettet run
Praktisk anbefalet rækkefølge når der ikke findes kendte cases:
1. Kør discovery-run
2. Vælg et fundet `IncidentId`, `Sagsnr` eller `Kontonr`
3. Kør målrettet `DRYRUN`
4. Kør derefter kontrolleret `RUN` med de nødvendige toggles

### 12. Vigtige regler for dette job
- `BrugerArealSager` skal være en reel Dataverse-bruger ved normal `DRYRUN` og `RUN`, men ikke nødvendigvis ved `VERIFYCRM` eller `EnableDiscoveryRun=true`.
- `AllowPartialRun=true` er en sikkerhedsgate. Uden den må den lokale write-vej ikke frigives.
- `EnableDiscoveryRun=true` er read-only og bruges til at finde kandidater, ikke til at gennemføre closeout.
- `TilladSendTilDigitalPost=true` er ikke det samme som fuld digital-post-portering; i denne leverance er det kun stub/staging, når `EnableDigitalPostStubRun=true`.
- `EnableCarryForwardArealRun=true`, `EnableDeleteZeroRegnskabRun=true` og `EnableArealSumQueueRun=true` er tekniske seams og dokumenterer ikke i sig selv fuld GI-paritet.

## Teststatus pr. kategori

### Unit
- `KontoAfslutArealSagerLetterMergeDataTests`
- `KontoAfslutArealSagerSettingsValidatorTests`
- `KontoAfslutArealSagerCloseoutPayloadFactoryTests`
- `KontoAfslutArealSagerRequestFactoryTests`

### Smoke
- `ServiceRegistrySmokeTests`

### Integration
- ikke oprettet i denne iteration

### Regression
- ikke oprettet i denne iteration

## Autoritativ statusmatrix

| Felt | Status |
|---|---|
| GI NuGet-status | Ikke close-out. Ny lokal hovedvej etableret for scan, brev og closeout-seams |
| GI assembly-status | Ikke verificeret i denne leverance |
| Moderniseringsstatus | Fase 7 discovery-run + digital-post-staging + areal-carry-forward + AREALSUM2KONTO-seam konverteret til ny Batchjobs-målarkitektur |
| Afkoblingsstatus | GI-fri scan-/brevvej etableret; lokal partial CRM-write etableret; queue-closeout, direct-closeout og areal-carry-forward seams etableret |
| Clean/arkitekturstatus | God og testbar struktur med tydelige write-gates |
| Shared-status | Shared-konsolideret for runtime/config/failure-komponenter; jobspecifik composition root forbliver lokal |
| Config-/deploystatus | `appsettings.local.json` og template medfølger |
| Build-/teststatus | Grøn build/test rapporteret lokalt efter seneste shared-fixrunde |
| Restpunkter og undtagelser | GI-ækvivalent areal-closeout, ekstern digital post og dokumenteret forretningseffekt mangler fortsat; discovery-mode er read-only, fase 7 carry-forward er ikke fuld GI-beregning, og digital post er kun staged som note |
| Verifikationsgrundlag | Kildeanalyse, lokal strukturkonvertering, DRYRUN/partial-run-observationer og grøn lokal teststatus |

## Næste anbefalede step

1. Brug `EnableDiscoveryRun=true` og `DiscoveryLimit` til at finde kandidater read-only uden kendt kontonr.
2. Brug derefter om nødvendigt `ForceIncidentId`, `ForceSagsnummer` eller `ForceKontonr` til smal teknisk verificering.
3. Kør `VERIFYCRM`, `DRYRUN` og derefter kontrolleret `RUN` med `AllowPartialRun=true`.
4. Når datagrundlag findes: verificér `EnableCloseoutQueueRun=true` og/eller `EnableDirectIncidentCloseoutRun=true`.
5. Verificér fase-5-toggles separat: `EnableCarryForwardArealRun`, `EnableDeleteZeroRegnskabRun` og `EnableArealSumQueueRun`.
6. Portér derefter den jobspecifikke GI-ækvivalente arealberegning/arealopdatering fra `LukArealCheckSmaaDifRequest` til lokal Dataverse-vej.
7. Før close-out: kør build/test, VERIFYCRM, DRYRUN, RUN uden partial-flag og root bin-scan.

## Leveranceoversigt

- nyt job under `jobs/dk.gi.app.konto.afslutarealsager/`
- nyt testprojekt under `jobs/dk.gi.app.konto.afslutarealsager/dk.gi.app.konto.afslutarealsager.Tests/`
- `legacy-reference/` med originale filer fra legacy-jobbet
- start-/fase-2-/fase-3-/fase-4-evidens under `docs/jobs/evidence/dk.gi.app.konto.afslutarealsager/`


## Supplerende evidens

- `docs/jobs/evidence/shared-konsolidering-og-teststatus-20260417.md`

## Eksterne kilder og integrationer

| Kilde | Formål | Adfærd | Teknisk adgangsvej | Settings / kilder | Modes | Risici / forbehold |
|---|---|---|---|---|---|---|
| Dataverse / CRM | Læser sager, konti og opfølgningsdata samt udfører write-faser afhængigt af gates. | Læs i DRYRUN, læs + skriv i RUN-gates | Lokal Dataverse-adapter | CRM-settings fra App Configuration / environment | VERIFYCRM, DRYRUN, RUN | Flere write-scenarier er gatede og skal dokumenteres særskilt pr. modekombination. |
| Service Bus | Valgfrie queue-forløb for closeout/arealsum styret af feature toggles. | Publicér når relevante gates er slået til | Lokal queue-adapter | Systemforvaltede Service Bus-settings + lokale gates | RUN | Ikke alle queue-spor er nødvendigvis aktive i samme iteration. |
| Digital Post / notifikationer | Ikke-porterede eller stubbede spor afhængigt af gate. | Afhænger af toggles | Lokale adapters / stubspor | Jobspecifikke toggles | RUN | Skal behandles som særskilte scenarier i testcase-dokumentet. |

## Lokal settings-normalisering pr. 2026-04-20

- `appsettings.local.json` er normaliseret til batchjobs-standarden med obligatoriske lokale standardnøgler.
- systemforvaltede CRM- og Service Bus-kerneværdier er fjernet fra lokalfilen og forventes nu fra systemkilder.
- jobspecifikke lokale overrides er bevaret i det omfang de understøtter sikker lokal `DRYRUN`/diagnostik.

## Testcases for eksterne kald

Særskilt testcase-dokument: `docs/jobs/testcases/dk.gi.app.konto.afslutarealsager.external-calls.md`

