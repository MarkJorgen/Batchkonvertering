# dk.gi.app.konto.kontoejerLuk

## Stamdata

- Job: `dk.gi.app.konto.kontoejerLuk`
- Iteration: dokumenteret DRYRUN-close-out-forberedelse med grøn teststatus og åben slutverificering
- Legacy-kilde: `GI-Batch/src/dk.gi.app.konto.kontoejerLuk`
- Legacy request analyseret: `RequestResponse/KontoejerLukRequest.cs`
- Konverteringsretning: lokal Dataverse-læsning + lokal write-adapter for `ap_kontoejer`

## Kort beskrivelse

Legacy-jobbet finder slettede konti (`ap_statusframapper = 2`), læser åbne `ap_kontoejer` for hver konto og sætter `ap_slutdato` til kontoens `ap_sidsteregnskabsdato`, når feltet endnu ikke er udfyldt.

Den nye vej er modelleret som:
1. lokal App Configuration-bootstrap og startup-diagnostik
2. lokal Dataverse-læsning af slettede konti
3. lokal læsning af åbne kontoejere pr. konto
4. lokal planlægning af lukkedatoer i application-laget
5. opdatering af `ap_slutdato` i RUN-mode

## Driftsprofil

- Modes: `VERIFYCRM`, `DRYRUN`, `RUN`
- Primær bootstrap: `AZURE_APPCONFIG_CONNECTIONSTRING`
- Lokal override: `appsettings.local.json`
- Lokal template: `appsettings.local.template.jsonc`
- Legacy schedule videreført fra `settings.job`

## Arkitekturstatus

- tynd `Program.cs`
- lokal composition root i `ServiceRegistry`
- application-lag for orkestrering og closure-planlægning
- infrastructure-adapters for Dataverse, config, runtime guard og failure notification
- legacy-filer lagt i `legacy-reference/` og ikke brugt som aktiv runtimevej

## Porteret forretningsadfærd i denne iteration

Følgende er eksplicit modelleret i startkonverteringen:
- læsning af slettede konti via `ap_konto`
- læsning af åbne kontoejere via `ap_kontoejer`
- lokal filtrering så kun åbne kontoejere med gyldig sidste regnskabsdato planlægges til lukning
- write-fase der sætter `ap_slutdato` på kontoejer
- DRYRUN-spor uden write

## Afhængighedsoversigt

### Direkte i ny vej
- `Microsoft.PowerPlatform.Dataverse.Client`
- `Microsoft.PowerPlatform.Dataverse.Client.Dynamics`
- `shared/Gi.Batch.Shared`

### Legacy beholdt som reference
- originale legacy-filer under `jobs/dk.gi.app.konto.kontoejerLuk/legacy-reference/`

## Config-/settingsoversigt

Kerneindstillinger i denne iteration:
- `Mode`
- `CrmConnectionTemplate`
- `CrmServerName`
- `CrmClientId`
- `CrmClientSecret`
- `CrmAuthority`
- `AuthorityMode` / `CrmAuthorityMode`
- `SecondsToSleep`
- `MaxWaitCount`
- `TimeOutMinutter`
- `modtagereEmail`

## Teststatus pr. kategori

### Unit
- `KontoejerLukPlannerTests`
- `KontoejerLukSettingsValidatorTests`

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
- ingen dokumenteret parity-sammenligning mod legacy runtime endnu

## Autoritativ statusmatrix

| Felt | Status |
|---|---|
| GI NuGet-status | Ny lokal hovedvej etableret; endelig NuGet-close-out ikke særskilt dokumenteret i denne iteration |
| GI assembly-status | Top-level GI bin-scan rapporteret bestået uden fund af ekstra `dk.gi*.dll` |
| Moderniseringsstatus | Startkonverteret til ny Batchjobs-struktur |
| Afkoblingsstatus | Lokal Dataverse-hovedvej etableret for læsning og write |
| Clean/arkitekturstatus | God og testbar struktur med lokal planlægningslogik |
| Shared-status | Shared-konsolideret for runtime/config/failure-komponenter; jobspecifik composition root forbliver lokal |
| Config-/deploystatus | `appsettings.local.json` og template medfølger |
| Build-/teststatus | Grøn build/test rapporteret lokalt; `DRYRUN` rapporteret bestået |
| Restpunkter og undtagelser | Mangler dokumenteret `VERIFYCRM`, `RUN`-evidens, faktisk write-effekt og parity-verifikation |
| Verifikationsgrundlag | Kildeanalyse, portering, rapporteret grøn build/test, rapporteret `DRYRUN` og rapporteret GI bin-scan |

## Næste anbefalede step

1. Kør og dokumentér `VERIFYCRM` som separat forbindelsesevidens.
2. Kør kontrolleret `RUN` mod kendt case.
3. Dokumentér faktisk write-effekt på `ap_kontoejer.ap_slutdato`.
4. Sammenlign adfærden mod legacy-sporet før endelig close-out.

## Close-out-vurdering

Jobbet kan i denne iteration lukkes dokumentationsmæssigt som **mellem-close-out / close-out-forberedt**:
- den nye GI-frie hovedvej er etableret
- `DRYRUN` er rapporteret bestået
- GI bin-scan er rapporteret bestået
- men endelig slutverificering mangler fortsat, fordi `RUN`-effekt, `VERIFYCRM` og parity mod legacy endnu ikke er dokumenteret

## Leveranceoversigt

- nyt job under `jobs/dk.gi.app.konto.kontoejerLuk/`
- nyt testprojekt under `jobs/dk.gi.app.konto.kontoejerLuk/dk.gi.app.konto.kontoejerLuk.Tests/`
- `legacy-reference/` med originale filer fra legacy-jobbet


## Supplerende evidens

- `docs/jobs/evidence/shared-konsolidering-og-teststatus-20260417.md`

## Eksterne kilder og integrationer

| Kilde | Formål | Adfærd | Teknisk adgangsvej | Settings / kilder | Modes | Risici / forbehold |
|---|---|---|---|---|---|---|
| Dataverse / CRM | Læser slettede konti og åbne kontoejere samt opdaterer `ap_slutdato` i RUN. | Læs i DRYRUN, læs + skriv i RUN | Lokal Dataverse-adapter | CRM-settings fra App Configuration / environment | VERIFYCRM, DRYRUN, RUN | Faktisk write-effekt er endnu ikke dokumenteret i close-out. |
| Azure App Configuration / environment | Leverer bootstrap og CRM-settings. | Læs | JobConfigurationLoader + Azure App Configuration | `AZURE_APPCONFIG_CONNECTIONSTRING` og miljøkilder | Alle modes | Lokalfilen indeholder kun lokale overrides og obligatoriske standardnøgler. |
| Failure notification | Valgfri fejlnotifikation. | Console fallback / evt. mail | Gi.Batch.Shared failure notification | `EnableFailureEmail`, `modtagereEmail`, evt. Azure.Email.* | Alle modes | Mail er ikke hovedverifikationsspor. |

## Lokal settings-normalisering pr. 2026-04-20

- `appsettings.local.json` er normaliseret til batchjobs-standarden med obligatoriske lokale standardnøgler.
- systemforvaltede CRM- og Service Bus-kerneværdier er fjernet fra lokalfilen og forventes nu fra systemkilder.
- jobspecifikke lokale overrides er bevaret i det omfang de understøtter sikker lokal `DRYRUN`/diagnostik.

## Testcases for eksterne kald

Særskilt testcase-dokument: `docs/jobs/testcases/dk.gi.app.konto.kontoejerLuk.external-calls.md`

