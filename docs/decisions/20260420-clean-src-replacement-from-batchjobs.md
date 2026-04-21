# 2026-04-20 – clean src replacement from Batchjobs

Denne leverance bruger den seneste Batchjobs-leverance som source of truth for de konverterede jobs og GI-Batch som legacy-repository.

Følgende jobs er erstattet rent i `GI-Batch/src/` ved først at slette den tilsvarende legacy-jobmappe og derefter kopiere den konverterede mappe ind:

- `dk.gi.app.contact.lassox.ophoer`
- `dk.gi.app.contact.registrering.optaelling`
- `dk.gi.app.contact.registreringudloebne.optaelling`
- `dk.gi.app.contact.selskab`
- `dk.gi.app.ejendom.tjekejerskifte`
- `dk.gi.app.konto.afslutarealsager`
- `dk.gi.app.konto.beregnsatserlog.slet`
- `dk.gi.app.konto.kontoejerLuk`
- `dk.gi.app.konto.startarealtjek`

Der er desuden tilføjet:
- `Directory.Build.props` i repo-roden
- `nuget.config` i repo-roden
- `shared/Gi.Batch.Shared`
- `docs/` fra Batchjobs-sporet
- `PROJEKTBESKRIVELSE.txt` opdateret til GI-Batch/src-modellen
- Batchjobs-verifikationsscripts under `scripts/`

Der er bevidst **ikke** lavet overlay ind i de erstattede jobmapper. Gamle jobfiler som `Program.template.cs`, `Program.Plus.cs`, `Program_App_*`, gamle `packages.config`-spor, `legacy-reference/` og jobspecifikke legacy-solutionfiler er derfor fjernet sammen med den slettede legacy-jobmappe for de erstattede jobs.
