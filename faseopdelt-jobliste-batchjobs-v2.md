# Faseopdelt jobliste for Batchjobs-projektet – styringsopdatering

Formål: opdateret faseopdeling efter styringsbeslutning om, at allerede eksisterende konverterede jobs anses som lukkede, også selv om de ikke er VERIFYCRM-kontrolleret.

## Styringsbeslutning

Alle allerede eksisterende konverterede jobs i den aktuelle Batchjobs-portefølje behandles fremadrettet som lukkede.

Det betyder i praksis:
- tidligere krav om VERIFYCRM som formelt closeout-krav fastholdes som generel projektregel
- men de jobs, der allerede er konverteret i den nuværende portefølje, betragtes historisk/styringsmæssigt som lukkede, selv hvis VERIFYCRM ikke er dokumenteret
- næste fase starter derfor ikke med yderligere lukning af de allerede konverterede jobs, men med de næste åbne jobs i porteføljen

## Jobs der nu behandles som lukkede

1. `dk.gi.app.contact.registrering.optaelling`
2. `dk.gi.app.contact.registreringudloebne.optaelling`
3. `dk.gi.app.contact.lassox.ophoer`
4. `dk.gi.app.contact.selskab`
5. `dk.gi.app.konto.startarealtjek`
6. `dk.gi.app.konto.kontoejerLuk`
7. `dk.gi.app.ejendom.tjekejerskifte`

Disse jobs bruges fortsat som referencekilder for:
- målarkitektur
- shared-komponenter
- settingsmodel
- startup-diagnostik
- runtime artifact verification
- dokumentationsmønster

## Næste fase – aktive jobs

Næste fase består af de resterende åbne jobs i den nuværende portefølje:

### Fase N1 – tungere specialspor
- `dk.gi.app.konto.afslutarealsager`
- `dk.gi.app.konto.beregnsatserlog.slet`

## Prioriteret rækkefølge i næste fase

### 1. `dk.gi.app.konto.beregnsatserlog.slet`
Begrundelse:
- er allerede teknisk konverteret længere end rå analyse
- DRYRUN er bestået
- RUN er påbegyndt men ikke fuldført
- egner sig derfor til hurtigere færdiggørelse end et helt nyt spor

### 2. `dk.gi.app.konto.afslutarealsager`
Begrundelse:
- er mere kompleks
- har flere modes og toggles
- bør håndteres som særskilt specialspor efter beregnsatserlog.slet

## Praktisk kildeopdeling for næste fase

### Fast basis-kilde
- `PROJEKTBESKRIVELSE.txt`
- `shared/Gi.Batch.Shared/`
- `docs/templates/`
- `docs/decisions/`
- lukkede referencejobs

### Fasekilde for næste aktive fase
- `jobs/dk.gi.app.konto.beregnsatserlog.slet/`
- `jobs/dk.gi.app.konto.beregnsatserlog.slet.Tests/`
- `docs/jobs/dk.gi.app.konto.beregnsatserlog.slet.md`
- `docs/jobs/evidence/dk.gi.app.konto.beregnsatserlog.slet/`

- `jobs/dk.gi.app.konto.afslutarealsager/`
- `jobs/dk.gi.app.konto.afslutarealsager.Tests/`
- `docs/jobs/dk.gi.app.konto.afslutarealsager.md`
- `docs/jobs/evidence/dk.gi.app.konto.afslutarealsager/`

## Operativ start på næste fase

Næste fase startes sådan:

1. Færdiggør `dk.gi.app.konto.beregnsatserlog.slet`
   - luk RUN-/mellemtilstandssporet eller closeout som dokumenteret teknisk mellemtilstand
   - opdatér jobdokumentation, eksterne kilder og testcase-spor

2. Tag derefter `dk.gi.app.konto.afslutarealsager`
   - afgræns modes/toggles tydeligt
   - fasthold særskilt dokumentation for partial-run, closeout-queue og øvrige specialspor
   - luk som teknisk closeout eller ærlig mellemtilstand

## Kort styringskonklusion

Med denne beslutning er de allerede konverterede jobs ikke længere blocker for fremdrift.

Den næste reelle fase i projektet er derfor:
- først `dk.gi.app.konto.beregnsatserlog.slet`
- derefter `dk.gi.app.konto.afslutarealsager`
