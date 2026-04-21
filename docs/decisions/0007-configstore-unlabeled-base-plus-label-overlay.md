# 0007 - Config Store bruger ulabelede nøgler som standard

Pilotjobbet læser Config Store uden label-filtrering.

Begrundelse:
- de aktuelle CRM- og mailsettings i Azure App Configuration ligger som `(No label)`
- `ConfigStoreLabel` gav falske mangler i `RUN`
- den oprindelige GI.Batch-adfærd peger på almindelige settings-navne uden eksplicit label-styring i jobkoden

Konsekvens:
- `ConfigStoreLabel` udgår af pilotens standardkonfiguration
- fælles miljøbasis læses som ulabelede nøgler
- lokale overrides sker fortsat via `appsettings.local.json`
