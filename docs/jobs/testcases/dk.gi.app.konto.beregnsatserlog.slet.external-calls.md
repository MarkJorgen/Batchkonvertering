# dk.gi.app.konto.beregnsatserlog.slet – testcase for eksterne kald

Dette dokument supplerer jobdokumentationen med tekniske testcases for eksterne integrationer og mode-afhængig adfærd.

| Testcase-id | Navn | Forudsætninger | Mode | Input / trigger | Forventet eksternt kald | Forventet log / diagnostik | Forventet teknisk effekt / forbehold |
|---|---|---|---|---|---|---|---|
| TC-01 | VERIFYCRM connectivity | Gyldig systemkonfiguration og lokale standardnøgler | VERIFYCRM | Start job | Dataverse connect verificeres; ingen delete | Startup-/connect-diagnostik viser succes | Teknisk forbindelsesevidens; ingen sletteeffekt |
| TC-02 | DRYRUN kandidatscan | Gyldig konfiguration og læsbare data | DRYRUN | Kør standardjob | Kandidater læses; ingen delete | Resumé viser kandidater og `Deleted=0` | Teknisk flowverifikation uden sideeffekt |
| TC-03 | RUN delete-flow | Sikker driftssituation eller afgrænset population | RUN | Kør kontrolleret RUN | Delete udføres mod Dataverse | Resumé og log viser sletninger | Skal ikke regnes som close-out-evidens før fuldført og dokumenteret |

Bemærk:

- `appsettings.local.json` bruges kun til lokale overrides og ikke til systemforvaltede CRM-/Service Bus-kerneværdier.
- `VERIFYCRM` er forbindelsestest; `DRYRUN` er teknisk flow uden ekstern effekt; `RUN` er den mode hvor write/publicering kan forekomme.
