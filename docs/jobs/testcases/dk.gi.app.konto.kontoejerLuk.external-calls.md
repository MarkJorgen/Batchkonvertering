# dk.gi.app.konto.kontoejerLuk – testcase for eksterne kald

Dette dokument supplerer jobdokumentationen med tekniske testcases for eksterne integrationer og mode-afhængig adfærd.

| Testcase-id | Navn | Forudsætninger | Mode | Input / trigger | Forventet eksternt kald | Forventet log / diagnostik | Forventet teknisk effekt / forbehold |
|---|---|---|---|---|---|---|---|
| TC-01 | VERIFYCRM connectivity | Gyldig systemkonfiguration og lokale standardnøgler | VERIFYCRM | Start job uden forretningsmæssig write-trigger | Dataverse connect verificeres; ingen write/publicering | Startup-/connect-diagnostik viser succes | Teknisk forbindelsesevidens; ingen forretningseffekt |
| TC-02 | DRYRUN læseflow | Gyldig konfiguration og læsbare data | DRYRUN | Kør standardjob | Dataverse læsninger udføres; ingen write/publicering | Resumé viser kandidater uden effekt | Teknisk flowverifikation uden sideeffekt |
| TC-03 | RUN write-flow | Kendt testcase eller sikker population | RUN | Kør kontrolleret RUN | Dataverse write udføres | Resumé og joblog viser opdaterede records | Faktisk write-effekt skal evt. verificeres særskilt i CRM |

Bemærk:

- `appsettings.local.json` bruges kun til lokale overrides og ikke til systemforvaltede CRM-/Service Bus-kerneværdier.
- `VERIFYCRM` er forbindelsestest; `DRYRUN` er teknisk flow uden ekstern effekt; `RUN` er den mode hvor write/publicering kan forekomme.
