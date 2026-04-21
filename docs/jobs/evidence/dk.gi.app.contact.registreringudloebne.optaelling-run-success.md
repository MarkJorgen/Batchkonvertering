# RUN evidence – dk.gi.app.contact.registreringudloebne.optaelling

## Resultat
- Status: PASS
- Mode: RUN
- Dato: 2026-04-16
- Kilde: lokal kørsel rapporteret i projektchatten

## Centrale observationer
- startup-diagnostik viste alle CRM-settings som `FOUND`
- `ServiceBusBaseUrl`, `ServiceBusQueueName` og `ServiceBusLabel` var ikke sat som hårde startup-defaults
- Dataverse-connect lykkedes
- Service Bus base/SAS blev resolved via CRM `config_configurationsetting`
- workflowet gennemførte hele den nye lokale Dataverse + Service Bus-adapter-vej uden exception

## Centrale loglinjer
- `RUN-mode aktiv. Kalder lokal Dataverse + Service Bus-erstatning for runout-registreringer.`
- `Service Bus resolved via crm config_configurationsetting.`
- `Workflow gennemført via local dataverse + servicebus adapter. Scannet=379. Publiceret=0. Mode=RUN. registreringid=<alle>. Ingen afsluttede treklip fundet.`

## Tolkning
Denne evidens beviser teknisk RUN-success på den nye vej. Den beviser ikke endnu funktionel parity mod legacy, men den viser at job og config/runtime-spor er kommet forbi startup, Dataverse-connect og execution path.
