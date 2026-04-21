# RUN evidence – dk.gi.app.contact.registrering.optaelling

## Resultat
- Status: PASS
- Mode: RUN
- Dato: 2026-04-16
- Kilde: lokal kørsel rapporteret i projektchatten

## Centrale observationer
- startup-diagnostik viste alle CRM-settings som `FOUND`
- legacy-kompatibel secret-dekryptering blev anvendt:
  - `legacy decrypt applied=Ja`
- Dataverse-connect lykkedes
- begge workflow-trin blev kørt:
  - `CloseExpiredTreklipOwnerRegistrations()`
  - `CreateJobsForContactRegistrerings()`

## Centrale loglinjer
- `Kalder CloseExpiredTreklipOwnerRegistrations().`
- `Kalder CreateJobsForContactRegistrerings(). Scope=alle registreringer`
- `Workflow gennemført via local legacy seam. Lukkede treklip/ejerregistreringer=True. Dannede kontaktjobs=True. Mode=RUN. registreringid=<alle>.`

## Bemærkning
Logteksten indeholder stadig enkelte historiske betegnelser som `legacy seam`, men kørslen repræsenterer den nye source-baserede Dataverse-vej og ikke den gamle GI-runtimevej.
