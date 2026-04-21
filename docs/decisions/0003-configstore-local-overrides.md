# 0003 - Azure Config Store med lokale overrides i batchjobs

## Beslutning

Batchjobs skal kunne køre i udvikling mod Azure Config Store som basis, men med lokal override af enkelte settings.

Konfigurationsrækkefølgen er derfor:

1. `App.config`
2. Azure Config Store
3. environment variables
4. `appsettings.local.json`
5. args

## Begrundelse

Den gamle model er vanskelig at gennemskue, fordi settings kommer implicit via flere spor.

Den nye model gør disse ting eksplicitte:

- om Config Store er slået til
- hvordan connection string sættes
- hvilken label der bruges
- at lokal override-fil vinder over Azure
- at args stadig kan bruges til runtime-parametre

## Konsekvens

- udviklere kan bruge miljøets rigtige settings i udvikling
- lokale følsomme eller støjende værdier som `modtagereEmail` kan overstyres uden at kopiere alt
- `settings.job` bevares som WebJob-driftsartefakt og bruges ikke som generel runtime-konfiguration


Den konkrete pilotleverance indeholder `appsettings.local.example.json` som skabelon. Den rigtige `appsettings.local.json` oprettes lokalt af udvikleren og committes ikke.
