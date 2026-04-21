# dk.gi.app.konto.regnskab.slet - DRYRUN 2026-04-20

## Formål
Dokumentere første tekniske kørsel på den nye GI-Batch `src`-erstatningsvej.

## Observeret log
```text
[DIAG] Startup-diagnostik for dk.gi.app.konto.regnskab.slet
[DIAG] Mode=DRYRUN
[DIAG] AuthorityMode=AsConfigured
[DIAG] RuntimeEngine=Modern
[DIAG] CrmServerName=FOUND
[DIAG] CrmClientId=FOUND
[DIAG] CrmClientSecret=FOUND
[DIAG] CrmAuthority=FOUND
[DIAG] ServiceBusQueueName=crmpluginjobs
[DIAG] ServiceBusLabel=KontoDiv
[INFO] Job færdigt. Mode=DryRun, SelectedAccounts=1, Published=0, ConnectivityVerified=True
```

## Konklusion
- startup-diagnostik bestået
- CRM-connectivity verificeret
- kandidatoptælling gennemført på ny vej
- Service Bus-settings resolved og synlige i diagnostikken
- ingen publicering udført, som forventet i `DRYRUN`

## Statusvurdering
Jobbet er nu **DRYRUN-verificeret close-out-forberedt mellemtilstand**. `RUN` og faktisk queue-effekt er fortsat ikke dokumenteret.
