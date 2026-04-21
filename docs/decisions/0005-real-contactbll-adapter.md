# 0005 – Reel ContactBLL-adapter i pilotjobbet

## Beslutning
Pilotjobbet `dk.gi.app.contact.registrering.optaelling` flyttes fra stubbet RUN-placeholder til en reel legacy-adapter i jobprojektets infrastruktur.

## Begrundelse
Den hidtidige pilot kunne debugges og konfigureres lokalt, men i RUN-mode var CRM-sporet stadig kun en placeholder. For at fortsætte pilotkonverteringen på en teknisk meningsfuld måde skal de to faktiske legacy-trin kunne kaldes bag en testbar adapter.

## Konsekvens
- GI-pakker er stadig nødvendige i denne iteration
- GI-koblingen er nu afgrænset til et smallere infrastrukturspor
- DRYRUN bevares som lokal sikkerhedsvej
- næste reduktionsblok kan fokusere på at minimere eller erstatte GI-typerne bag adapteren
