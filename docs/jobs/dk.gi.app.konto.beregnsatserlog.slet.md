# dk.gi.app.konto.beregnsatserlog.slet

## Formål
Slette `ap_beregnsatserlog`-records, som er ældre end det konfigurerede antal år (`AntalAar`).

## Kildesporet fra legacy
Legacy-jobbet opbygger et `SletBeregnSatserLogRequest`, kalder manager-metoden `HentAlleOprettetFoerDato(query_Years_OlderThan)` og sletter derefter alle fundne records én for én. GI-Nugets viser, at manager-metoden blot er en `QueryExpression` mod `ap_beregnsatserlog` med kolonnerne `ap_beregnsatserlogid` og `createdon` samt filteret `createdon OlderThanXYears`. Derefter udføres almindelig delete pr. record.

## Konverteringsstatus
**Status:** Mellemtilstand.

Det nye spor indeholder:
- tynd `Program.cs`
- lokal settingsmodel med `VERIFYCRM`, `DRYRUN` og `RUN`
- application-orchestrator uden GI-request/response-typer
- Dataverse-repository som erstatter `Ap_BeregnSatserLogManager.HentAlleOprettetFoerDato`
- unit-tests for validator og orchestrator
- `appsettings.local.json` og template

## Aktuel close-out-vurdering
Jobbet er teknisk konverteret og har bestået startup-diagnostik, CRM-connectivity og `DRYRUN`. Runtime artifact verification fandt ingen `dk.gi*.dll` i de validerede runtime-outputmapper. `RUN` blev påbegyndt, men sluttede før færdiggørelse og kan derfor ikke bruges som close-out-evidens. Jobbet skal derfor aktuelt dokumenteres som **mellemtilstand** og ikke som full eller conditional close-out.

## Autoritativ statusmatrix
- **GI NuGet-status:** Ikke endeligt dokumenteret i denne status
- **GI assembly-status:** Bestået top-level runtime artifact verification; ingen `dk.gi*.dll` fundet i de validerede runtime-outputmapper
- **Moderniseringsstatus:** Ny lokal jobstruktur etableret med application/infrastructure-opdeling
- **Afkoblingsstatus:** Workflowet er porteret væk fra GI request/manager-sporet i ny kode
- **Clean/arkitekturstatus:** Forbedret i forhold til legacy; fortsat med åbne driftsforhold omkring `RUN`
- **Shared-status:** Genbruger kun generiske shared-komponenter for settings/logging/diagnostik
- **Config-/deploystatus:** Lokal settingsfil og template medfølger; startup-diagnostik verificeret
- **Build-/teststatus:** Grøn build rapporteret lokalt; teststatus ikke selvstændigt opdateret i dette dokument
- **DRYRUN-status:** Bestået; `Candidates=23202`, `Deleted=0`, `ConnectivityVerified=True`
- **VERIFYCRM-status:** Ikke dokumenteret som selvstændig særskilt kørsel i denne status
- **RUN-status:** Påbegyndt, men ikke fuldført; ikke godkendt som evidens
- **Faktisk effekt:** Ikke verificeret
- **Restpunkter og undtagelser:** `RUN`-adfærd ved stor datamængde kræver forbedret telemetri/begrænsning eller særskilt kontrolleret driftstest
- **Verifikationsgrundlag:** Startup-diagnostik, bestået `DRYRUN` med connectivity, top-level GI runtime artifact verification, observeret ikke-fuldført `RUN`
- **Close-out-kategori:** Mellemtilstand

## Bevist
- Startup-diagnostik gennemføres korrekt
- Relevante CRM-settings findes og indlæses korrekt
- CRM-connectivity er verificeret på den nye vej
- `DRYRUN` er gennemført succesfuldt
- Kandidatudvælgelsen returnerede et reelt datagrundlag (`23202` kandidater)
- Ingen `dk.gi*.dll` blev fundet i de validerede runtime-outputmapper

## Ikke bevist
- Fuld gennemførsel af `RUN`
- Faktisk sletteeffekt
- Stabil og kontrolleret driftsprofil ved større datamængder
- Endelig close-out-egnet effektverifikation

## Begrundelse for mellemtilstand
`RUN` blev påbegyndt, men sluttede før færdiggørelse. Derfor foreligger der ikke tilstrækkelig evidens til hverken full close-out eller conditional close-out. Jobbet står derfor aktuelt som **mellemtilstand**.

## Settingsoversigt
| Setting | Hvor den læses | Brug |
|---|---|---|
| `Mode` | `Program.cs` via `SletBeregnSatserLogSettingsFactory` | `VERIFYCRM`, `DRYRUN`, `RUN` |
| `AntalAar` | `SletBeregnSatserLogSettingsFactory` | Grænse for `OlderThanXYears` |
| `TimeOutMinutter` | `SletBeregnSatserLogSettingsFactory` | Legacy-parameter bevaret; endnu ikke brugt direkte i Dataverse-klienten |
| `SecondsToSleep` | `SletBeregnSatserLogSettingsFactory` | Legacy sync-/mutex-parameter bevaret til senere fan-out |
| `MaxWaitCount` | `SletBeregnSatserLogSettingsFactory` | Legacy sync-/mutex-parameter bevaret til senere fan-out |
| `CrmConnectionTemplate` | `CrmConnectionStringFactory` | Danner Dataverse connection string |
| `CrmServerName` | `CrmConnectionStringFactory` | CRM hostname |
| `CrmClientId` | `CrmConnectionStringFactory` | Client id |
| `CrmClientSecret` | `CrmConnectionStringFactory` | Client secret |
| `CrmAuthority` | `CrmConnectionStringFactory` | Authority/tenant |
| `modtagereEmail` | `SletBeregnSatserLogSettingsFactory` | Reserveret fejlmailsetting |

## Næste anbefalede step
1. Forbedr `RUN`-telemetri og progress logging.
2. Indfør eventuel begrænsning pr. kørsel, fx `MaxDeletesPerRun`.
3. Gennemfør ny kontrolleret validering af `RUN`, hvis det er driftsmæssigt forsvarligt.
4. Opdatér status efter ny evidens.

## Eksterne kilder og integrationer

| Kilde | Formål | Adfærd | Teknisk adgangsvej | Settings / kilder | Modes | Risici / forbehold |
|---|---|---|---|---|---|---|
| Dataverse / CRM | Finder og sletter `ap_beregnsatserlog`-records ældre end konfigureret antal år. | Læs i DRYRUN, læs + delete i RUN | Lokal Dataverse-repository | CRM-settings fra App Configuration / environment | VERIFYCRM, DRYRUN, RUN | RUN er endnu ikke dokumenteret som fuldført close-out-evidens. |
| Azure App Configuration / environment | Leverer bootstrap og CRM-settings. | Læs | JobConfigurationLoader + Azure App Configuration | `AZURE_APPCONFIG_CONNECTIONSTRING` og miljøkilder | Alle modes | Systemforvaltede CRM-værdier er fjernet fra lokalfilen. |
| Failure notification | Valgfri fejlnotifikation. | Console fallback / evt. mail | Gi.Batch.Shared failure notification | `EnableFailureEmail`, `modtagereEmail`, evt. Azure.Email.* | Alle modes | Mail er sekundært i forhold til sletteworkflowet. |

## Lokal settings-normalisering pr. 2026-04-20

- `appsettings.local.json` er normaliseret til batchjobs-standarden med obligatoriske lokale standardnøgler.
- systemforvaltede CRM- og Service Bus-kerneværdier er fjernet fra lokalfilen og forventes nu fra systemkilder.
- jobspecifikke lokale overrides er bevaret i det omfang de understøtter sikker lokal `DRYRUN`/diagnostik.

## Testcases for eksterne kald

Særskilt testcase-dokument: `docs/jobs/testcases/dk.gi.app.konto.beregnsatserlog.slet.external-calls.md`

