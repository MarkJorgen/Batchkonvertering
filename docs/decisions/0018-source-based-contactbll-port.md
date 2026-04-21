# 0018 – Source-baseret ContactBLL-portering i piloten

## Beslutning
Pilotens RUN-/VERIFYCRM-spor fastholdes som lokal Dataverse-adapter, men den konkrete workflow-logik for contact.registrering.optaelling behandles nu som source-baseret portering udledt af GI-Nugets-kildekoden frem for som gættet runtime-seam.

## Baggrund
GI-Nugets gjorde følgende kildefiler tilgængelige som autoritativt grundlag for workflowet:
- ContactBLL-Registreringer.cs
- ContactBllRegisteringResponse.cs
- ContactManager.V2.cs
- EjerRegistreringManager.V2.cs

Det viste, at ContactBLL-metoderne er tynde wrappers omkring konkret query- og update-logik, og at den rigtige slutretning er lokal portering af workflow/logik – ikke action-gætteri.

## Konsekvens
- Piloten fortsætter med lokal Dataverse-adapter.
- Wording ændres fra legacy ContactBLL-flow til lokal source-baseret ContactBLL-erstatning.
- Comparison-sporet bevares som særskilt baseline-/verifikationsspor, men blokerer ikke i sig selv for at færdigportere den lokale implementation.
- Endelig close-out afventer stadig funktionel parity og sammenligningsklart legacy-spor.
