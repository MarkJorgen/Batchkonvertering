# Root bin GI assembly scan – 2026-04-17

## Formål

Dokumentere top-level GI assembly-status separat fra GI NuGet-status og functional RUN-status ved scanning fra repo-roden mod samtlige projekters `bin`-output.

## Scope

Scanningen blev kørt fra repo-roden `C:\Users\MPAGI\source\repos\BatchJobs` mod samtlige projekter under:

- `jobs/`
- `shared/`
- `legacy/`
- `artifacts/`

Der blev søgt efter ekstra runtime-assemblies med mønstret:

- `dk.gi*.dll`

Jobgenes egne primære `exe`-filer indgik ikke som fundkriterium.

## Resultat

Scanningen returnerede:

`Ingen ekstra dk.gi DLL assemblies fundet i bin-output.`

## Tolkning

Det understøtter, at de aktuelle konverterede job ikke længere medbringer synlige ekstra `dk.gi*.dll` i normal `bin`-output.

Det erstatter ikke:

- functional RUN-verifikation
- parity-vurdering mod legacy
- anden eventuel specialiseret runtime-/deploy-verifikation

## Relevante job i den aktuelle bølge

- `dk.gi.app.contact.registrering.optaelling`
- `dk.gi.app.contact.registreringudloebne.optaelling`
- `dk.gi.app.contact.lassox.ophoer`
- `dk.gi.app.contact.selskab`
- `dk.gi.app.konto.startarealtjek`
