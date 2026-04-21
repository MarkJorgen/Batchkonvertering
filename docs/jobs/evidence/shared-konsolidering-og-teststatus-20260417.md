# Shared-konsolidering og samlet teststatus – 2026-04-17

## Status

Følgende er rapporteret opnået lokalt i den seneste shared-fixrunde:

- den samlede testpakke er grøn
- shared-konsolideringen er fastholdt i `shared/Gi.Batch.Shared`
- de konverterede jobs genbruger fortsat de løftede runtime-/config-/failure-komponenter via tynde lokale wrappers

## Omfang

Denne status bruges som dokumentationsgrundlag for:
- opdateret shared-status i de konverterede jobdokumenter
- opdateret build-/teststatus for `dk.gi.app.konto.kontoejerLuk`
- opdateret build-/teststatus for `dk.gi.app.ejendom.tjekejerskifte`
- opdateret build-/teststatus for `dk.gi.app.konto.afslutarealsager`

## Vigtig afgrænsning

Grøn teststatus ændrer ikke i sig selv funktionel close-out-status for jobs, der fortsat mangler:
- `VERIFYCRM`
- `RUN`
- dokumenteret write-/queue-effekt
- parity mod legacy

## Konsekvens for projektstatus

Efter denne dokumentationsslutning gælder:
- shared-konsolideringen er dokumenteret som gennemført i batchjobs-sporet
- alle aktuelle testprojekter er rapporteret grønne lokalt
- funktionel slutverificering skal stadig dokumenteres separat pr. job
