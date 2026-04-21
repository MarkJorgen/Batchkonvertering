# dk.gi.app.konto.startarealtjek – testcase for eksterne kald

Dette dokument supplerer jobdokumentationen med tekniske testcases for eksterne integrationer og mode-afhængig adfærd.

| Testcase-id | Navn | Forudsætninger | Mode | Input / trigger | Forventet eksternt kald | Forventet log / diagnostik | Forventet teknisk effekt / forbehold |
|---|---|---|---|---|---|---|---|
| TC-01 | VERIFYCRM connectivity | Gyldig systemkonfiguration og lokale standardnøgler | VERIFYCRM | Start job uden write/publicering | Dataverse connect verificeres; ingen queue-publicering | Startup-/connect-diagnostik viser succes | Teknisk forbindelsesevidens; ingen forretningseffekt |
| TC-02 | DRYRUN læseflow uden publicering | Gyldig konfiguration og læsbare data | DRYRUN | Kør standardjob | Dataverse læsninger udføres; ingen write/publicering | Resumé viser kandidater uden ekstern effekt | Skal dokumentere fravær af queue-kald |
| TC-03 | RUN med write/publicering | Kendt testcase eller sikker population | RUN | Kør kontrolleret RUN | Dataverse write og/eller Service Bus-publicering udføres | Resumé og log viser effekt/publikation | Nedstrøms effekt kan kræve separat verificering |

Bemærk:

- `appsettings.local.json` bruges kun til lokale overrides og ikke til systemforvaltede CRM-/Service Bus-kerneværdier.
- `VERIFYCRM` er forbindelsestest; `DRYRUN` er teknisk flow uden ekstern effekt; `RUN` er den mode hvor write/publicering kan forekomme.
