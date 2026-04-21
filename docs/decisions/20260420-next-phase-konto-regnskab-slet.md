# 2026-04-20 - næste fase: dk.gi.app.konto.regnskab.slet

## Beslutning
Efter dokumenteret DRYRUN for `dk.gi.app.konto.satser.slet` fortsætter næste fase med `dk.gi.app.konto.regnskab.slet` i samme rene `src`-erstatningsmodel.

## Begrundelse
- `dk.gi.app.konto.regnskab.slet` er et af de resterende legacy-job i `GI-Batch/src`
- GI-Nugets-kilde viser en afgrænset forretningsvej: kandidatquery + kø-publicering
- jobbet passer godt til projektets faseopdeling: ren erstatning først, runtime-verifikation bagefter

## Konsekvens
- legacy-jobmappen er fjernet og erstattet 1:1 af ny konverteret mappe
- satser.slet-dokumentationen er opdateret med bestået DRYRUN
- næste verifikationsspor er lokal build/test og `VERIFYCRM`/`DRYRUN` for `dk.gi.app.konto.regnskab.slet`


## Efterfølgende teknisk validering
- `DRYRUN` er efterfølgende bestået på den nye vej
- observeret resultat: `SelectedAccounts=1`, `Published=0`, `ConnectivityVerified=True`
- kandidaten er dermed ikke længere kun startkonverteret, men DRYRUN-verificeret close-out-forberedt mellemtilstand
