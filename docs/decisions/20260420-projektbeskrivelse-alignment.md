# Projektbeskrivelse-alignment 2026-04-20

Denne note opsummerer repo-opdateringerne udført for at bringe eksisterende batchjobs i linje med den nyeste projektbeskrivelse.

## Udførte ændringer

- Synkroniseret nyeste `PROJEKTBESKRIVELSE.txt` ind i repoets styrende kopier.
- Normaliseret lokale settings-filer på tværs af alle eksisterende jobprojekter.
- Tilføjet manglende `appsettings.local.json`, `appsettings.local.example.json` og `appsettings.local.template.jsonc` hvor nødvendigt.
- Fjernet systemforvaltede CRM-/Service Bus-kerneværdier fra lokale filer.
- Opdateret `README.md` til den aktuelle portfolio og settings-standard.
- Udvidet jobdokumenter med eksplicit oversigt over eksterne kilder og testcase-reference.
- Oprettet testcase-dokumenter for eksterne kald under `docs/jobs/testcases/`.

## Lokal settings-standard anvendt

```json
{
  "Mode": "DRYRUN",
  "EnableFailureEmail": false,
  "EnableLocalDebugLogging": true,
  "LocalDebugLogPath": "C:\\Temp\\GI.Batch\\LocalLogs",
  "modtagereEmail": "mpa.gi@hotmail.com"
}
```

## Jobmatrix

| Job | appsettings.local.json | example | template | systemkerneværdier fjernet lokalt | testcase-dokument |
|---|---|---|---|---|---|
| `dk.gi.app.contact.lassox.ophoer` | Ja | Ja | Ja | Ja | `docs/jobs/testcases/dk.gi.app.contact.lassox.ophoer.external-calls.md` |
| `dk.gi.app.contact.registrering.optaelling` | Ja | Ja | Ja | Ja | `docs/jobs/testcases/dk.gi.app.contact.registrering.optaelling.external-calls.md` |
| `dk.gi.app.contact.registreringudloebne.optaelling` | Ja | Ja | Ja | Ja | `docs/jobs/testcases/dk.gi.app.contact.registreringudloebne.optaelling.external-calls.md` |
| `dk.gi.app.contact.selskab` | Ja | Ja | Ja | Ja | `docs/jobs/testcases/dk.gi.app.contact.selskab.external-calls.md` |
| `dk.gi.app.ejendom.tjekejerskifte` | Ja | Ja | Ja | Ja | `docs/jobs/testcases/dk.gi.app.ejendom.tjekejerskifte.external-calls.md` |
| `dk.gi.app.konto.afslutarealsager` | Ja | Ja | Ja | Ja | `docs/jobs/testcases/dk.gi.app.konto.afslutarealsager.external-calls.md` |
| `dk.gi.app.konto.beregnsatserlog.slet` | Ja | Ja | Ja | Ja | `docs/jobs/testcases/dk.gi.app.konto.beregnsatserlog.slet.external-calls.md` |
| `dk.gi.app.konto.kontoejerLuk` | Ja | Ja | Ja | Ja | `docs/jobs/testcases/dk.gi.app.konto.kontoejerLuk.external-calls.md` |
| `dk.gi.app.konto.startarealtjek` | Ja | Ja | Ja | Ja | `docs/jobs/testcases/dk.gi.app.konto.startarealtjek.external-calls.md` |
