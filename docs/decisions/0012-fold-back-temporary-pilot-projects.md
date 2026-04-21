# 0012 – Fold back temporary pilot projects

## Beslutning
De midlertidige projekter `Contracts` og `LegacyAdapter` fjernes igen, og deres kode foldes tilbage ind i selve pilotjobbet.

## Begrundelse
Mellemstationen med ekstra projekter gjorde det muligt at tage et stort, men sikkert reduktionsstep og bevise en hård projektgrænse. Slutretningen for piloten er dog uden ekstra projekter.

## Konsekvens
- Jobprojektet bærer igen den lokaliserede GI-kobling under `src/Infrastructure/Crm/Legacy/`.
- GI-pakkerne er ikke væk endnu fra det samlede job.
- Til gengæld er slutstrukturen nu tættere på den ønskede form uden ekstra projekter.
- Næste reduktionsblok skal fjerne eller erstatte den resterende GI-brug uden at genindføre ekstra projekter.
