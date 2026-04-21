# Konto afslutarealsager - fase 4 direct-closeout

## Hvad denne iteration ændrer

- tilføjer `EnableDirectIncidentCloseoutRun=true` som eksplicit fase-4-gate for direkte lukning af incident i Dataverse
- tilføjer smalle override-filtre: `ForceIncidentId`, `ForceSagsnummer`, `ForceKontonr`
- tilføjer nul-hit diagnostik for owner/subject/followup-kæden ved standardscan
- retter stale logtekst fra fase 2/3 til fase 4
- opdaterer lokal settings-template og request-mapping
- retter dato-forventning i `KontoAfslutArealSagerLetterMergeDataTests`

## Hvad der fortsat ikke er porteret

- Digital Post
- GI-ækvivalent arealberegning og `ap_areal`-opdatering
- 0-regnskabssletning
- fuld close-out evidens for forretningseffekt

## Ærlig status

Dette er fortsat en mellemtilstand. Fase 4 gør write-/closeout-sporet mere komplet og mere testbart, men er ikke dokumenteret som fuld GI-ækvivalent sluttilstand.
