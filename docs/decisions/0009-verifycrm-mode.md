# 0009 - VERIFYCRM som pilotmellemode

## Beslutning
Piloten får et særskilt `VERIFYCRM`-mode.

## Begrundelse
Vi ønsker at kunne verificere rigtig CRM-opkobling uden at køre de egentlige ContactBLL-operationer i `RUN`, især når der endnu ikke ønskes smoketest mod konkrete registreringer.

## Konsekvens
- `DRYRUN` bruges fortsat til lokal arkitektur- og flowtest uden CRM-kald
- `VERIFYCRM` bruges til at validere reelle CRM-settings og `CrmContext.IsReady`
- `RUN` bruges først, når både config og CRM-opkobling er verificeret og egentlig operation ønskes
