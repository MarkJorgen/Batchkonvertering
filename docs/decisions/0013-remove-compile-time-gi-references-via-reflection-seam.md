# 0013 – Fjern compile-time GI-referencer via reflection seam

Pilotjobbet er flyttet fra direkte compile-time brug af GI-pakker til et lokalt lokal Dataverse SDK-implementation under `src/Infrastructure/Crm/Legacy/`.

## Hvad er ændret
- direkte `PackageReference` til `dk.gi.crm.bll.lib`, `dk.gi.crm.data.lib`, `dk.gi.crm.lib` og `dk.gi.lib` er fjernet fra pilotjobbet
- `CrmContext`, `ContactBLL` og `KrypteringFjern` kaldes nu via reflection
- et valgfrit setting `[udgået] LegacyGiAssemblyDirectory` kan bruges til at pege på en lokal mappe med GI DLL-filer under debug eller overgangsdrift

## Formål
- fjerne compile-time GI-kobling uden at indføre flere projekter
- holde den resterende runtime-afhængighed samlet ét sted

## Status
Dette er stadig ikke endelig GI-frihed, fordi runtime fortsat kan kræve GI assemblies, men compile-time GI-pakkekoblingen er fjernet fra pilotjobbet.
