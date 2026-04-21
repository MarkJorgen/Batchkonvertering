# 0004 - Lokal debug-logning som valgfri udviklerfunktion

## Beslutning

Azure-logning er fortsat den ønskede primære driftsretning, men i pilot- og udviklingsfasen må lokal debug-logning kunne slås til via lokal override-fil.

Standardadfærden er derfor:

- lokal debug-logning er slået fra
- lokal debug-logning aktiveres kun via `appsettings.local.json`
- lokal debug-logning kræver en eksplicit `LocalDebugLogPath`
- lokale logging-settings committes ikke som aktiv standard

## Begrundelse

Udviklingsfasen kræver ofte hurtig lokal fejlsøgning, før den endelige Azure-logning er fuldt implementeret og valideret i hele platformssporet.

Den valgte model giver:

- mindre støj i standardopsætningen
- tydelig afgrænsning mellem drift og lokal debugging
- testbar og eksplicit konfiguration

## Konsekvens

- `EnableLocalDebugLogging=false` er standard
- `LocalDebugLogPath` bruges kun når lokal debug-logning er slået til
- pilotjobbet kan skrive til lokal logfil i udvikling uden at gøre lokal logning til et krav i almindelig drift
