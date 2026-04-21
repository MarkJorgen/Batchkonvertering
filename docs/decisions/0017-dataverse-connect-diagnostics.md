# 0017 – Dataverse connect diagnostics

## Beslutning
Pilotjobbet udvides med mere præcis diagnostik omkring Dataverse-opkobling.

## Baggrund
RUN er nået forbi den tidligere GI-runtimefejl, men fejlede efterfølgende med den generiske besked `Failed to connect to Dataverse`.

## Ændring
Der logges nu eksplicit:
- CrmServerName
- CrmAuthority
- om CrmClientId er sat
- om CrmClientSecret er sat
- sanitiseret connection string uden secret
- LastError og LastException fra ServiceClient ved connect-fejl

## Konsekvens
Næste RUN/VERIFYCRM-forsøg bør afgøre, om fejlen skyldes format, credentials, authority, miljøadgang eller anden Dataverse-klientfejl.
