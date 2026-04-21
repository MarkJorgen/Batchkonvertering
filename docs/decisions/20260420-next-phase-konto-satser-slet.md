# 2026-04-20 - næste fase med dk.gi.app.konto.satser.slet

## Beslutning
Næste fase gennemføres som en ren `src`-erstatning af legacy-jobbet `dk.gi.app.konto.satser.slet` i `GI-Batch`.

## Begrundelse
- jobmetoden `Ap_SatserManager.HentSatserIdForSletTilAar(int aar)` kunne analyseres direkte i GI-Nugets-kildekoden
- workflowet er afgrænset og består af lokal kandidatfremsøgning + delete pr. record
- jobtypen passer godt til den allerede bevist struktur fra `dk.gi.app.konto.beregnsatserlog.slet`

## Gennemført
- legacy-jobmappen er erstattet rent under `src/`
- nyt testprojekt og ny dokumentation er oprettet
- næste konkrete step er lokal build/test + `VERIFYCRM` + `DRYRUN`
