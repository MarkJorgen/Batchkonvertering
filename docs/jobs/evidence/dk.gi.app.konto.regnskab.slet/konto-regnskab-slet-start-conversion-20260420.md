# dk.gi.app.konto.regnskab.slet - fase 1 startkonvertering

## Formål
Dokumentere at legacy-jobbet `dk.gi.app.konto.regnskab.slet` er erstattet rent i `GI-Batch/src` af en ny konverteret jobmappe uden overlay-rester.

## Gennemført i denne iteration
- legacy-mappen under `src/dk.gi.app.konto.regnskab.slet/` er slettet og erstattet 1:1 af ny jobmappe
- ny `Program.cs`, application/infrastructure-lag og testprojekt er etableret
- lokal kandidatquery og kø-publiceringsmodel er porteret fra GI-Nugets-sporet
- lokal settingsmodel, testcase-dokument og jobdokument er oprettet

## Efterfølgende status
Efter denne startkonvertering er første tekniske `DRYRUN` dokumenteret i særskilt evidensfil `konto-regnskab-slet-dryrun-20260420.md`.

## Konklusion
Denne evidens beskriver kun startkonverteringen. Aktuel samlet status fremgår af jobdokumentationen, hvor jobbet nu er løftet til **DRYRUN-verificeret close-out-forberedt mellemtilstand**.
