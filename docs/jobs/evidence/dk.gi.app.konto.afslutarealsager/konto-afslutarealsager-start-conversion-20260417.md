# dk.gi.app.konto.afslutarealsager – fase 1 startkonvertering

## Formål

Denne evidensfil dokumenterer, at jobsporet er løftet ind i Batchjobs-målarkitekturen som fase 1-konvertering.

## Hvad der er gjort

- oprettet nyt jobprojekt i `jobs/`
- oprettet nyt testprojekt
- flyttet legacy-filer til `legacy-reference/`
- genbrugt Aspose-licens og brevskabelon som embedded resources
- etableret ny Dataverse-read-klient for sager, konto, ejendom og kontakt
- etableret ny brev-flettemodel og PDF-generering i hukommelsen
- lagt eksplicit write-barriere ind foran `RUN`

## Bevidst sikkerhedsbarriere

`RUN` er i denne leverance blokeret som standard, fordi den jobspecifikke areal-closeout-logik endnu ikke er porteret.

Det er en bevidst sikkerhedsbeslutning for at undgå halvkonverteret produktionsadfærd.

## Åbne restpunkter

- portering af aktivitet/opgave/upload
- portering af digital post
- portering af areal-closeout (`LukArealCheckSmaaDifRequest`)
- faktisk build/test/RUN-verifikation lokalt
