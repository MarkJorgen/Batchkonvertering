# dk.gi.app.konto.afslutarealsager - fase 3 queue-closeout-forberedelse (2026-04-17)

## Formål

Portere næste sikre blok uden testcase:

1. behold lokal Dataverse-scan og brevvej
2. behold aktivitet + note + aktivitetslukning som partial run
3. lokaliser closeout-publicering til Service Bus som separat seam

## Hvad der er ændret

- nyt lokalt Service Bus-closeout publisher-spor
- payload matcher legacy-intentionen: `Mode=Incident`, `action=luksagaktiviteter`, `beskrivelse=Luk areal check`
- Service Bus kan resolves via `config_configurationsetting`
- failure-notifikation under `DRYRUN` og `VERIFYCRM` er slået fra

## Hvad der ikke er bevist endnu

- faktisk publicering til `KontoDiv` er ikke verificeret i denne leverance
- GI-ækvivalent arealberegning og `ap_areal`-opdatering er ikke porteret endnu
- slutlukning/close-out er derfor ikke opnået
