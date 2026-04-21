# dk.gi.app.konto.startarealtjek – start conversion evidence – 2026-04-17

## Formål

Dokumentere at næste fan-out-job er oprettet i Batchjobs-strukturen som en reel startkonvertering og ikke kun som note i projektbeskrivelsen.

## Leveret i denne iteration

- nyt jobprojekt `dk.gi.app.konto.startarealtjek`
- nyt testprojekt `dk.gi.app.konto.startarealtjek.Tests`
- `legacy-reference/` med de originale legacy-filer
- lokal Dataverse-adapter for scanning, vurdering og opdatering af `ap_emneforarealtjek`
- lokal queue-publicering til `crmpluginjobs` / `ArealTjekKonto`
- `appsettings.local.json` og `appsettings.local.template.jsonc`

## Ikke endnu verificeret

- lokal build
- lokale tests
- VERIFYCRM
- DRYRUN
- RUN
- root bin GI assembly scan

## Korrekt status

Dette dokument beskriver den **historiske startkonvertering**. Jobbet er siden blevet RUN-verificeret og close-out-opdateret i separat evidensfil `konto-startarealtjek-run-closeout-20260417.md`.
