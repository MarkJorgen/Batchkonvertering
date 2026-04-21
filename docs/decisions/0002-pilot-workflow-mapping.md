# 0002 - Pilot workflow mapping for dk.gi.app.contact.registrering.optaelling

## Beslutning

Første pilot omsætter ikke hele legacy-templatekæden 1:1. I stedet flyttes det konkrete workflow til tre eksplicitte lag:

- request parse
- orchestrator
- CRM gateway

## Begrundelse

Legacy-koden blander i dag bootstrap, config, CRM-oprettelse, Serilog-initiering, single-instance og forretningsflow i partial `Program_*`-filer.

Det konkrete domæneflow er dog relativt lille:

- valgfrit `registreringid`
- luk afsluttede treklip/ejerregistreringer
- find og opret jobs for kontakter med registreringer

Det gør jobbet egnet som første pilot for en tynd `Main`-model.

## Konsekvens

I denne iteration er CRM-grænsen stadig en stub/placeholder.

Det næste rigtige skridt er en adapter, der kapsler:

- `SetAfsluttedeTreklipEjerRegistreringerInaktive()`
- `FindAndCreateJobsForContactWithRegistrerings(registreringid)`

bag et lokalt interface uden at hele `Program_App_callback.cs` bevares som runtime-entrypoint.
