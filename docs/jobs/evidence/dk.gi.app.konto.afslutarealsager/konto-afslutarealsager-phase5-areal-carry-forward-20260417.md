# Konto afslutarealsager - fase 5 areal carry-forward og AREALSUM2KONTO

## Hvad denne iteration ændrer

- tilføjer `EnableCarryForwardArealRun=true` som eksplicit fase-5-gate for lokalt carry-forward af åbent `ap_areal`
- tilføjer `EnableDeleteZeroRegnskabRun=true` som valgfri sletning af 0-regnskab (`ap_regnskab` med årsagskode `02`)
- tilføjer `EnableArealSumQueueRun=true` som valgfri publicering af `AREALSUM2KONTO` efter oprettelse af nyt areal
- udvider statusopsamlingen med lukkede/oprettede arealer, slettede 0-regnskaber og publicerede AREALSUM2KONTO-job

## Hvad fase 5 er - og ikke er

Fase 5 er et **lokalt carry-forward-seam**. Det betyder, at den åbne `ap_areal` lukkes på sidste regnskabsdato, og et nyt areal oprettes med næste dags periodestart og kopierede feltværdier fra det åbne areal.

Det er **ikke** en fuld GI-ækvivalent arealberegning. Den del mangler fortsat.

## Fortsat ikke porteret

- Digital Post
- GI-specialregler for beregning af nyt areal
- fuld dokumenteret forretningseffekt i RUN
- end-state close-out
