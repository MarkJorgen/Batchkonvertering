# 0006 - Startup-diagnostik for RUN-forudsætninger

## Beslutning
Pilotjobbet for `dk.gi.app.contact.registrering.optaelling` skal ved opstart udskrive en kort og sikker diagnostik før settings-validering.

## Begrundelse
Når piloten bevæger sig fra `DRYRUN` til `RUN`, er den hyppigste fejl ikke selve CRM-koden men manglende eller forkert løste settings. Diagnostikken gør label, bootstrap og RUN-krav synlige uden at udskrive secrets.

## Konsekvens
- færre blinde fejl i lokal debug
- tydeligere afgrænsning mellem config-fejl og CRM-fejl
- bedre dokumenteret overgang fra pilotens DRYRUN til RUN
