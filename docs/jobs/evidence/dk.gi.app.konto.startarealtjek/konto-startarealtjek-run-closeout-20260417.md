# dk.gi.app.konto.startarealtjek – RUN close-out evidence – 2026-04-17

## Resultat

Lokal RUN på den nye vej blev rapporteret bestået med følgende observerede resultat:

- startup-diagnostik gennemført med App Configuration-bootstrap
- Dataverse klient oprettet succesfuldt
- hele læse- og kvalificeringsflowet gennemført for `5377` konti
- write-fasen gennemført med opdatering af `ap_emneforarealtjek` på `210` konti
- Service Bus-settings resolved via CRM `config_configurationsetting`
- `1` konkret arealtjek-job publiceret til `crmpluginjobs` / `ArealTjekKonto`
- proces afsluttet med workflow-resumé på den nye vej

## Dokumenteret output

- `Scannet=5377`
- `Emner=4800`
- `IkkeEmner=577`
- `Publiceret=1`
- `Mode=RUN`

## Dokumenteret write- og queue-effekt

- `Opdaterede ap_emneforarealtjek på 210 konto(er).`
- `Publicerede 1 konto.startarealtjek job(s).`

## Publiceret konto

- `45-06362` (`ccba9019-54a7-dd11-99b9-000423e0dabd`)

## Bemærkning

Dette jobs egen write-evidens er både CRM-opdatering af `ap_emneforarealtjek` og efterfølgende queue-publicering. Det gør jobbet til et kombineret write-/queue-closeout sammenlignet med de øvrige fan-out-jobs.
