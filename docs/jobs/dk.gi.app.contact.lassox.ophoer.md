# dk.gi.app.contact.lassox.ophoer

## Stamdata

- Job: `dk.gi.app.contact.lassox.ophoer`
- Iteration: første fan-out efter `dk.gi.app.contact.registreringudloebne.optaelling`
- Udgangspunkt: referencearkitektur fra kontakt-/CRM-familien i Batchjobs
- Legacy-kilde: `GI-Batch/src/dk.gi.app.contact.lassox.ophoer`
- GI-kildekode brugt til analyse og portering: `ContactBLL-TjekLassoX.cs`, `ContactManager.V2.cs`, `AP_kontoejerManager.V2.cs`, `ap_reelejerManager.cs`

## Formål

Dette job afmelder LassoX for kontakter, der ikke længere er åbne kontoejere eller aktive reelle ejere.

Den lokale implementation mapper legacy-flowet sådan:

1. find kontakter med aktivt LassoX-abonnement
2. find åbne `ap_kontoejer`
3. find aktive `ap_reelejer`
4. behold abonnement hvis kontakten fortsat findes i en af ejerrelationerne
5. afmeld ellers abonnementet

## Arkitekturstatus

### Opnået i denne leverance
- Program-laget er simplificeret til tynd `Program.cs`
- lokal kontrakt mellem application og infrastructure er etableret
- startup-diagnostik og App Configuration-bootstrap er genbrugt fra referencearkitekturen
- legacy-kaldet til `ContactBLL.TjekKontaktpersonerLassoX(true)` er erstattet af lokal Dataverse-scan og lokal beslutningskerne
- skjult hardcoded simulation i legacy er løftet frem som eksplicit mode-styring (`DRYRUN` vs `RUN`)
- testprojekt er oprettet med Unit- og Smoke-spor
- legacy-reference er medtaget som `.original`-filer under `legacy-reference/`

### Bevidst afgrænsning i denne leverance
- konverteringen er skrevet som GI-fri målarkitektur, men er ikke build-/runtime-valideret i denne container
- den gamle selskabsgren i legacy-koden er fortsat udeladt, i tråd med kommentaren i GI-kildekoden
- real-ejer-match følger den dokumenterede legacy-sammenligning på `ap_reelejerid`

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
- `modtagereEmail`
- `TimeOutMinutter`
- `SecondsToSleep`
- `MaxWaitCount`

## Autoritativ statusmatrix

- GI NuGet-status: væsentligt reduceret for dette workflow; ny lokal vej anvendes i RUN
- GI assembly-status: verificeret via root-scan af samtlige projekters `bin`-output; ingen ekstra `dk.gi*.dll` fundet
- Moderniseringsstatus: gennemført for dette workflow
- Afkoblingsstatus: høj; ny lokal Dataverse-adapter og lokal beslutnings-/write-kerne anvendes i RUN
- Clean/arkitekturstatus: god og ensartet med referencearkitekturen
- Shared-status: shared-konsolideret for runtime/config/failure-komponenter
- Config-/deploystatus: appsettings og bootstrap-model er etableret og lokalt verificeret
- Build-/teststatus: grøn lokalt efter seneste shared-fixrunde
- Functional RUN-status: bestået på ny vej
- Restpunkter og undtagelser:
  - parity mod legacy-reference er ikke endeligt dokumenteret
  - observationen `BeholdtSomReelEjer=0` bør vurderes i et senere parity-/forretningseffektspor, hvis relevant
- Verifikationsgrundlag: lokal grøn build/test, lokal RUN-log med write-effekt, root bin GI assembly scan, legacy-kode, GI-Nugets kildekode og referencearkitektur

## Testspor i leverancen

- Unit:
  - beslutningslogik for kontoejer / reel ejer / afmelding
  - gateway-adfærd for `DRYRUN` kontra `RUN`
  - settings-validator
- Smoke:
  - service registry i `DRYRUN` uden Config Store

## Seneste RUN-verifikation

Lokal RUN på den nye vej blev rapporteret bestået med følgende centrale observationer:

- Dataverse klient oprettet succesfuldt
- læsning af aktive LassoX-kontakter gennemført
- læsning af åbne kontoejere gennemført
- læsning af reelle ejere gennemført
- beslutningsfase gennemført
- write-fase gennemført
- proces afsluttet med `exit code 0`

Dokumenteret resultat:

- `Scannet=3050`
- `BeholdtSomKontoejer=3037`
- `BeholdtSomReelEjer=0`
- `KandidaterTilAfmelding=13`
- `Opdateret=13`
- `Mode=RUN`

## Næste anbefalede step

1. dokumentér parity-vurdering mod legacy-reference, hvis forretningsmæssigt relevant
2. verificér konkret CRM-forretningseffekt for de 13 opdaterede kontakter, hvis dette ønskes som ekstra evidensspor
3. bevar jobmet som reference for næste søsterjob i samme mønsterfamilie

## Leveranceoversigt

- nyt job under `jobs/dk.gi.app.contact.lassox.ophoer/`
- nyt testprojekt under `jobs/dk.gi.app.contact.lassox.ophoer/dk.gi.app.contact.lassox.ophoer.Tests/`
- `legacy-reference/` med originale jobfiler
- opdateret `GI.Batch.sln`
- opdateret `README.md`


## 2026-04-17 runtime patch

- `GetRealOwnerContactIds()` følger nu den dokumenterede legacy-adfærd og matcher på `ap_reelejerid`.
- Beslutnings- og write-fase logger nu særskilt progress og slutsummering.
- `appsettings.local.json` harmoniseret med den etablerede lokale override-standard fra `dk.gi.app.contact.registrering.optaelling`.


## Evidens

- `docs/jobs/evidence/dk.gi.app.contact.lassox.ophoer/lassox-ophoer-run-closeout-20260417.md`
- `docs/jobs/evidence/root-bin-gi-assembly-scan-20260417.md`


## Slutstatus

**Jobbet er teknisk close-out-klar på den nye vej.**

Det betyder i denne leverance:
- grøn build/test er rapporteret lokalt
- RUN på ny vej er bestået
- faktisk write-effekt er dokumenteret (`Opdateret=13`)
- root bin-scan fandt ingen ekstra `dk.gi*.dll`

Jobbet beskrives endnu ikke som fuld end-state close-out, fordi parity mod legacy-reference fortsat er et særskilt evidensspor.

## Eksterne kilder og integrationer

| Kilde | Formål | Adfærd | Teknisk adgangsvej | Settings / kilder | Modes | Risici / forbehold |
|---|---|---|---|---|---|---|
| Dataverse / CRM | Læser LassoX-abonnementer, kontoejere og reelle ejere samt opdaterer abonnementstatus. | Læs i DRYRUN, læs + skriv i RUN | Lokal Dataverse-adapter | CRM-settings fra App Configuration / environment, lokal `CrmAuthorityMode` ved behov | VERIFYCRM, DRYRUN, RUN | Reel ejer-parity kan fortsat forbedres som separat evidensspor. |
| Azure App Configuration / environment | Leverer bootstrap- og CRM-kilder. | Læs | JobConfigurationLoader + Azure App Configuration | `AZURE_APPCONFIG_CONNECTIONSTRING` og miljøkilder | Alle modes | Lokalfilen er nu holdt fri for systemforvaltede CRM-værdier. |
| Failure notification | Valgfri fejlnotifikation. | Console fallback / evt. mail | Gi.Batch.Shared failure notification | `EnableFailureEmail`, `modtagereEmail`, evt. Azure.Email.* | Alle modes | Mail er ikke del af den dokumenterede runtimeeffekt. |

## Lokal settings-normalisering pr. 2026-04-20

- `appsettings.local.json` er normaliseret til batchjobs-standarden med obligatoriske lokale standardnøgler.
- systemforvaltede CRM- og Service Bus-kerneværdier er fjernet fra lokalfilen og forventes nu fra systemkilder.
- jobspecifikke lokale overrides er bevaret i det omfang de understøtter sikker lokal `DRYRUN`/diagnostik.

## Testcases for eksterne kald

Særskilt testcase-dokument: `docs/jobs/testcases/dk.gi.app.contact.lassox.ophoer.external-calls.md`

