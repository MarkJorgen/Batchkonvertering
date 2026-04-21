# dk.gi.app.contact.registrering.optaelling

## Formål med denne leverance

Dette dokument beskriver pilotjobbet efter den første komplette, succesfulde GI-frie auth-/RUN-verifikation på den nye vej.

Målet for piloten har været at etablere og bevise:

- tynd `Program.cs`
- ny composition root
- eksplicit og testbar konfigurationskæde
- `appsettings.local.json` som lokal override-fil
- startup-diagnostik og sikker connect-diagnostik
- lokal kontrakt mellem application og infrastructure
- source-baseret portering af ContactBLL-workflowet til ny Dataverse-vej
- unit-/smoke-/regressionstest som første testbase
- dokumentations- og evidensstruktur for senere bølger

## Legacy-reference

Originale filer fra den uploadede GI-Batch-kode ligger under:

- `jobs/dk.gi.app.contact.registrering.optaelling/legacy-reference/`

De er bevaret som reference og indgår ikke i build.

## Konfigurationsmodel

Konfiguration læses nu i denne rækkefølge:

1. `App.config`
2. Azure Config Store, hvis `UseConfigStore=true`
3. environment variables
4. `appsettings.local.json`, hvis filen findes
5. command-line arguments

Det betyder i praksis:

- udvikling kan hente miljøets basissettings fra Azure
- enkelte lokale værdier kan overrides uden at kopiere hele konfigurationen ned lokalt
- `modtagereEmail` kan sættes lokalt til udviklingsmail
- lokal debug-logning kan slås til lokalt uden at ændre standardopsætningen
- `settings.job` er fortsat deploy/WebJob-artefakt

### Lokal udvikling

Anbefalet lokale modes:

- `Mode=DRYRUN` for sikker flowtest uden CRM-kald
- `Mode=VERIFYCRM` for ren Dataverse-/auth-verifikation
- `Mode=RUN` for rigtig workflow-kørsel

Standard lokal fil er:

```json
{
  "Mode": "DRYRUN",
  "EnableFailureEmail": false,
  "EnableLocalDebugLogging": true,
  "LocalDebugLogPath": "C:\\Temp\\GI.Batch\\LocalLogs",
  "modtagereEmail": "mpa.gi@hotmail.com",
  "CrmAuthorityMode": "TenantBase"
}
```

## Arkitekturstatus

### Opnået
- `Program.cs` er reduceret til bootstrap og exit code mapping
- orchestration er flyttet til `src/Application/Services`
- konfigurationsindlæsning er gjort eksplicit og testbar
- startup-diagnostik gør RUN-forudsætninger og connect-shape synlige
- lokal Dataverse-adapter er etableret bag lokal kontrakt
- auth-regressionen i den nye vej er fundet og rettet via legacy-kompatibel secret-dekryptering
- `VERIFYCRM` på den nye vej er bestået
- `RUN` på den nye vej er bestået
- testprojekt er etableret og grønne tests er opnået

### Ikke endeligt opnået endnu
- funktionel parity mod legacy-referencevejen er ikke endeligt bevist
- comparison-sporet er fortsat ikke fuldt auth-/runtime-sammenligningsklart
- top-level GI runtime artifact verification er nu suppleret af root-scan mod samtlige projekters `bin`-output uden fund af ekstra `dk.gi*.dll`
- enkelte historiske navne/tekster i runtime-log og docs kan fortsat findes og bør ryddes helt i sidste close-out

## Autoritativ statusmatrix

- GI NuGet-status: Delvist opfyldt / aktiv ny vej uden direkte GI-pakker i workflowsporet
- GI assembly-status: Root bin-scan uden fund af ekstra `dk.gi*.dll`; særskilt runtime artifact verification foreligger også som evidens
- Moderniseringsstatus: Høj
- Afkoblingsstatus: Høj
- Clean/arkitekturstatus: Høj, men mindre navneoprydning kan stadig foretages
- Shared-status: Shared-konsolideret for runtime/config/failure-komponenter; jobspecifik workflowlogik forbliver lokal
- Config-/deploystatus: Etableret
- Build-/teststatus: Grøn
- Functional VERIFYCRM-status: Bestået på ny vej
- Functional RUN-status: Bestået på ny vej
- Comparison legacy baseline-status: Delvist / fortsat særskilt runtime-afklaringsspor
- Restpunkter:
  - vedligehold root bin GI assembly scan som løbende evidens ved nye close-out iterationer
  - dokumentér funktionel parity med konkrete CRM-observationer
  - ryd sidste historiske log-/navneterminologi

## Root cause-læring fra piloten

Den væsentligste konkrete kodefejl i den nye GI-frie vej var, at CRM-secret ikke blev behandlet på samme måde som i legacy-sporet.

Det korrekte mønster viste sig at være:
- hent `CrmClientSecret` fra App Configuration
- normalisér scalar-værdien sikkert
- anvend legacy-kompatibel dekryptering før connection string bygges

Før denne rettelse gav både `VERIFYCRM` og `RUN`:
- `AADSTS7000215 Invalid client secret provided`

Efter rettelsen blev følgende bevist:
- `VERIFYCRM` lykkes
- `RUN` lykkes

## Evidens

Se følgende filer:

- `docs/jobs/evidence/dk.gi.app.contact.registrering.optaelling-verifycrm-success.md`
- `docs/jobs/evidence/dk.gi.app.contact.registrering.optaelling-run-success.md`
- `docs/jobs/evidence/dk.gi.app.contact.registrering.optaelling-runtime-gi-verification.md`
- `docs/jobs/evidence/root-bin-gi-assembly-scan-20260417.md`

## Næste anbefalede step

1. Kør `scripts/Verify-GiAssemblies.ps1` mod den aktuelle succesfulde build
2. Verificér den faktiske forretningseffekt i CRM:
   - lukkede treklip/ejerregistreringer
   - opdaterede contact-tællere
3. Ryd sidste historiske “legacy”-terminologi i logs og dokumentation
4. Opdater derefter pilotens close-out-status som endelig eller næsten-endelig reference for næste job i samme mønsterfamilie

## Eksterne kilder og integrationer

| Kilde | Formål | Adfærd | Teknisk adgangsvej | Settings / kilder | Modes | Risici / forbehold |
|---|---|---|---|---|---|---|
| Dataverse / CRM | Læser og opdaterer kontakt-/registreringsdata i det porterede ContactBLL-workflow. | Læs + skriv i RUN | Lokal Dataverse-adapter via connection string factory | CRM-settings fra App Configuration / environment, lokal `CrmAuthorityMode` ved behov | VERIFYCRM, DRYRUN, RUN | Faktisk forretningseffekt bør fortsat verificeres særskilt. |
| Azure App Configuration / environment | Leverer systemforvaltede bootstrap- og CRM-værdier. | Læs | JobConfigurationLoader + Azure App Configuration | `AZURE_APPCONFIG_CONNECTIONSTRING` og miljøkilder | Alle modes | Lokalfil må ikke overskrive systemværdier med tomme værdier. |
| Failure notification | Valgfri fejlnotifikation ved fejl. | Console fallback / evt. mail | Gi.Batch.Shared failure notification | `EnableFailureEmail`, `modtagereEmail`, evt. Azure.Email.* | Alle modes | Mail kan være slået fra lokalt uden at påvirke hovedflowet. |

## Lokal settings-normalisering pr. 2026-04-20

- `appsettings.local.json` er normaliseret til batchjobs-standarden med obligatoriske lokale standardnøgler.
- systemforvaltede CRM- og Service Bus-kerneværdier er fjernet fra lokalfilen og forventes nu fra systemkilder.
- jobspecifikke lokale overrides er bevaret i det omfang de understøtter sikker lokal `DRYRUN`/diagnostik.

## Testcases for eksterne kald

Særskilt testcase-dokument: `docs/jobs/testcases/dk.gi.app.contact.registrering.optaelling.external-calls.md`

