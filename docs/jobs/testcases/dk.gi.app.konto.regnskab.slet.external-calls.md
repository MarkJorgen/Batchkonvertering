# dk.gi.app.konto.regnskab.slet - testcases for eksterne kald

## TC-REGNSKAB-SLET-001 - VERIFYCRM
- **Forudsætninger:** gyldige CRM-settings fra systemkilder
- **Mode:** `VERIFYCRM`
- **Input / trigger:** start job uden yderligere jobargumenter
- **Forventet eksternt kald:** Dataverse-connectivity etableres
- **Forventet fravær af kald:** ingen account-query og ingen Service Bus-publicering
- **Forventet log:** startup-diagnostik + connectivity OK
- **Forventet effekt:** ingen dataændringer

## TC-REGNSKAB-SLET-002 - DRYRUN
- **Forudsætninger:** gyldige CRM-settings og læsbar configsetting `app.konto.regnskab.konti.sletning.koersel`
- **Mode:** `DRYRUN`
- **Input / trigger:** start job med `Mode=DRYRUN`
- **Forventet eksternt kald:** query af kandidater i Dataverse
- **Forventet fravær af kald:** ingen Service Bus-publicering
- **Forventet log:** antal udvalgte konti til kø-publicering
- **Observeret 2026-04-20:** `SelectedAccounts=1`, `Published=0`, `ConnectivityVerified=True`
- **Forventet effekt:** ingen kø-beskeder oprettes

## TC-REGNSKAB-SLET-003 - RUN
- **Forudsætninger:** kontrolleret testcase-miljø og godkendt kø-vindue
- **Mode:** `RUN`
- **Input / trigger:** start job med `Mode=RUN`
- **Forventet eksternt kald:** query af kandidater + publicering til `crmpluginjobs` med label `KontoDiv`
- **Forventet log:** antal publicerede kø-beskeder
- **Forventet effekt:** ét kø-job pr. udvalgt konto
- **Kendte forbehold:** må kun køres hvor kø-publicering er driftsmæssigt afklaret
