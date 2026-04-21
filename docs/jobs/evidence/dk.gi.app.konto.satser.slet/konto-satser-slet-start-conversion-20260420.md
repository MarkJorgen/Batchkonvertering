# dk.gi.app.konto.satser.slet - fase 1 startkonvertering

## Formål
Dokumentere at legacy-jobbet `dk.gi.app.konto.satser.slet` er erstattet rent i `GI-Batch/src` af en ny konverteret jobmappe uden overlay-rester.

## Gennemført i denne iteration
- legacy-mappen under `src/dk.gi.app.konto.satser.slet/` er slettet og erstattet 1:1 af ny jobmappe
- ny `Program.cs`, application/infrastructure-lag og testprojekt er etableret
- lokal querylogik er porteret fra GI-Nugets-metoden `Ap_SatserManager.HentSatserIdForSletTilAar(int aar)`
- lokal settingsmodel, testcase-dokument og jobdokument er oprettet

## Ikke gennemført i denne iteration
- build/test
- `VERIFYCRM`
- `DRYRUN`
- `RUN`
- GI runtime artifact verification

## Konklusion
Jobbet er nu **startkonverteret mellemtilstand** og klar til lokal build/test og første tekniske validering på den rene `src`-branch.
