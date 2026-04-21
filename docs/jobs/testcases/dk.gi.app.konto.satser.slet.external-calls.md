# dk.gi.app.konto.satser.slet - testcases for eksterne kald

## TC-SATSER-001 - VERIFYCRM
- **Forudsætninger:** gyldige CRM-settings fra systemkilder
- **Mode:** `VERIFYCRM`
- **Input / trigger:** start job uden yderligere jobargumenter
- **Forventet eksternt kald:** Dataverse-connectivity etableres
- **Forventet fravær af kald:** ingen retrieve/delete af `ap_satser`
- **Forventet log:** startup-diagnostik + connectivity OK
- **Forventet effekt:** ingen dataændringer

## TC-SATSER-002 - DRYRUN
- **Forudsætninger:** gyldige CRM-settings, `SatsAar` sat til målår
- **Mode:** `DRYRUN`
- **Input / trigger:** start job med `Mode=DRYRUN`
- **Forventet eksternt kald:** retrieve af kandidat-`ap_satser`
- **Forventet fravær af kald:** ingen delete
- **Forventet log:** antal kandidater til sletning for `SatsAar`
- **Observeret 2026-04-20:** `Candidates=1`, `Deleted=0`, `ConnectivityVerified=True`
- **Forventet effekt:** ingen dataændringer

## TC-SATSER-003 - RUN
- **Forudsætninger:** kontrolleret testcase-miljø og godkendt slettevindue
- **Mode:** `RUN`
- **Input / trigger:** start job med `Mode=RUN`
- **Forventet eksternt kald:** retrieve + delete af kandidater i `ap_satser`
- **Forventet log:** antal slettede records
- **Forventet effekt:** kandidater uden undtagelsesflag for målåret fjernes
- **Kendte forbehold:** må kun køres hvor sletning er driftsmæssigt afklaret
