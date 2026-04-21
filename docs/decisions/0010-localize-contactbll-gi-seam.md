# 0010 – Lokaliser GI-koblingen omkring ContactBLL til ét smalt seam

## Beslutning
I pilotjobbet `dk.gi.app.contact.registrering.optaelling` samles den resterende direkte GI-brug nu i et smalt lokalt seam bestående af:

- `ContactRegistreringDataverseClientFactory`
- `ContactRegistreringDataverseClient`

## Formål
At reducere GI-referencefladen i pilotjobbet, så application/orchestrator-laget og workflow-orchestreringen ikke længere opretter eller kender `dk.gi.crm.CrmContext` eller `dk.gi.crm.data.bll.ContactBLL` direkte.

## Konsekvens
- `ContactRegistreringDataverseWorkflow` arbejder nu kun mod lokale interfaces.
- `ContactRegistreringCrmConnectionVerifier` arbejder nu også mod samme lokale factory.
- Den direkte GI-kobling er samlet ét sted i infrastrukturen og er derfor lettere at erstatte i næste reduktionsblok.
- GI-pakkerne er stadig til stede i pilotjobbet, men den aktive kobling er smallere og tydeligere.

## Næste step
Undersøg om samme seam kan genbruges i endnu et standardjob. Først når mønstret er bevist bredere, vurderes flytning til `Gi.Batch.Shared`.
