# dk.gi.app.konto.afslutarealsager – fase 2 partial-run plan

## Formål

Denne iteration løfter jobbet fra ren scan/brevforberedelse til kontrolleret partial CRM-write.

## Frigivet i fase 2

- oprettelse af aktivitet i CRM
- upload af PDF som note til aktivitet
- lukning af aktivitet
- slank lokal `appsettings.local.json` med kun lokale overrides
- `VERIFYCRM` kræver ikke længere `BrugerArealSager`

## Fortsat ikke frigivet

- Digital Post
- sagslukning
- GI-ækvivalent areal-closeout
- end-state RUN uden `AllowPartialRun`

## Forventet testsekvens

1. `Mode=VERIFYCRM`
2. `Mode=DRYRUN`
3. `Mode=RUN` + `AllowPartialRun=true`
4. CRM-kontrol af oprettet/lukket aktivitet og vedhæftet note
5. dokumentation og eventuel næste portering af closeout-spor
