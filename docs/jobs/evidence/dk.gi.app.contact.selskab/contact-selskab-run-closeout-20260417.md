# dk.gi.app.contact.selskab – RUN close-out evidence – 2026-04-17

## Resultat

Lokal RUN på den nye vej blev rapporteret bestået med følgende observerede resultat:

- startup-diagnostik gennemført med App Configuration-bootstrap
- Dataverse klient oprettet succesfuldt
- læsning af ejerrelationer gennemført på 3 sider
- Service Bus-settings resolved via CRM `config_configurationsetting`
- 2 konkrete KDK-opdateringsjob publiceret til `crmpluginjobs` / `KontoDiv`
- proces afsluttet med workflow-resumé på den nye vej

## Dokumenteret output

- `ObserveredeEjerrelationer=1146`
- `Kandidater=2`
- `Publiceret=2`
- `Mode=RUN`

## Publicerede selskaber

- `f0ad6205-73bc-e211-820e-005056843758` (`25970225`)
- `981270f6-583e-e911-8103-00505602060a` (`36560193`)

## Bemærkning

Dette jobs egen write-evidens er publicering af KDK-opdateringsjob. Den efterfølgende CRM-opdatering udføres nedstrøms af modtagerjobbet og er derfor ikke direkte del af dette jobs RUN-close-out.
