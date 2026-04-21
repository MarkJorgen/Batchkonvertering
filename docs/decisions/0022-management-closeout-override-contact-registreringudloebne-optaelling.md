# 0022 – Management close-out override for contact.registreringudloebne.optaelling

## Status
Accepted

## Context
Den normale projektregel kræver ærlig mellemtilstand, hvis legacy parity og faktisk CRM-forretningseffekt ikke er fuldt dokumenteret.

I denne iteration foreligger dog:
- grøn build/test
- bestået RUN på ny vej
- bestået GI runtime artifact verification på succesfuld build
- konsolideret config-/deploymodel
- opdateret dokumentation

## Decision
Jobbet `dk.gi.app.contact.registreringudloebne.optaelling` lukkes som full end-state close-out på management-niveau med accepterede undtagelser.

## Accepted exceptions
1. Funktionel parity mod legacy er ikke bevist
2. Comparison-sporet er ikke fuldt sammenligningsklart
3. Faktisk CRM-forretningseffekt er ikke fuldt dokumenteret

## Consequences
- close-out-status er styringsmæssigt lukket
- undtagelser videreføres som ikke-blokerende restforhold
- senere evidens- eller parity-arbejde må ikke omskrive den historiske close-out-beslutning, men kan supplere den
