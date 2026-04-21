# dk.gi.app

## Observationsnotat

Den uploadede repo bruger `dk.gi.app` som centralt fællesbibliotek på tværs af batchjobs. Biblioteket fungerer i praksis som både:

- runtime bootstrap
- config access
- trace/log helpers
- applikationsstatus
- fælles template-kæde

Det gør biblioteket nyttigt som analyseobjekt, men uegnet som direkte fremtidig shared-platform.

## Mål for opsplitning

Fremtidig struktur bør være:

- `Gi.Batch.Shared` for ren fremadrettet fælles kode
- `Gi.Batch.LegacyShim` for smalle overgangslag
- jobspecifik kode i de enkelte jobprojekter

## Første konkrete split i denne leverance

Flyttet/etableret som nye spor:

- execution result
- logger-interface
- konfigurationssources og loader
- legacy CRM factory interface
- legacy failure notifier
- legacy trace adapter

## Næste step

1. flyt generisk appstatus/resultat helt ud af `dk.gi.app`
2. flyt konfigurationslogik helt ud af `dk.gi.app`
3. flyt trace/log abstractions helt ud af `dk.gi.app`
4. efterlad kun smalle compat seams lokalt i jobprojektet eller flyt dem til shared når mønstret er bevist


## Opdatering i denne iteration – bredere shared-konsolidering

Følgende fælles komponenter er nu løftet ind i `Gi.Batch.Shared` og genbrugt fra de konverterede batchjobs via tynde lokale wrappers:

- `CrmScalarSettingNormalizer`
- `CompatCrmSecretDecryptor`
- `CrmConnectionStringFactory`
- `SingleInstanceGuard`
- `JobLoggerFactory`
- `FailureNotificationService`
- `ConsoleFailureNotifier`

Det betyder, at shared ikke længere kun indeholder basal config/logging/runtime-kontrakt, men også konkrete tværgående runtime-komponenter fra de konverterede jobs.

Begrænsning i denne iteration:
- `ServiceRegistry` er fortsat lokal pr. job, fordi composition root stadig binder jobspecifik orkestrering, gateways og adapters sammen.


## Dokumenteret status efter seneste shared-fixrunde

Shared-konsolideringen er nu dokumenteret som gennemført på tværs af de aktuelt konverterede batchjobs, og den samlede testpakke er efterfølgende rapporteret grøn lokalt.

Det betyder konkret:
- de løftede runtime-/config-/failure-komponenter i `Gi.Batch.Shared` er nu den dokumenterede fælles basis for de konverterede jobs
- jobspecifik composition root og domænelogik forbliver fortsat lokal
- shared-sporet er derfor ikke kun et designmål, men en faktisk gennemført del af den aktuelle batchjobs-leverance

Dette ændrer ikke i sig selv funktionel close-out-status for jobs, der stadig mangler `VERIFYCRM`, `RUN`, effektverifikation eller parity mod legacy.


## Supplerende evidens

- `docs/jobs/evidence/shared-konsolidering-og-teststatus-20260417.md`
