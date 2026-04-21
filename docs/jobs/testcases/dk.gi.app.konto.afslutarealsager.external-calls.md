# dk.gi.app.konto.afslutarealsager – testcase for eksterne kald

Dette dokument supplerer jobdokumentationen med tekniske testcases for eksterne integrationer og mode-afhængig adfærd.

| Testcase-id | Navn | Forudsætninger | Mode | Input / trigger | Forventet eksternt kald | Forventet log / diagnostik | Forventet teknisk effekt / forbehold |
|---|---|---|---|---|---|---|---|
| TC-01 | VERIFYCRM connectivity | Gyldig systemkonfiguration og gates slået fra | VERIFYCRM | Start job | Dataverse connect verificeres; ingen write/publicering | Startup-/connect-diagnostik viser succes | Teknisk forbindelsesevidens; ingen forretningseffekt |
| TC-02 | DRYRUN discovery/read-only | Gyldig konfiguration og sikre read-only gates | DRYRUN | Kør standardjob eller discovery-scenarie | Læsninger og planlægning udføres; ingen ekstern effekt | Resumé viser kandidater uden write/publicering | Bruges til teknisk validering af flow og population |
| TC-03 | RUN med snævre gates | Kendt case, sikre gates og eventuelle Force*-filtre | RUN | Kør kontrolleret RUN | Dataverse write og evt. queue-stier udføres efter gates | Log viser hvilke faser der reelt blev aktiveret | Hver gate-kombination bør dokumenteres separat ved driftstest |

Bemærk:

- `appsettings.local.json` bruges kun til lokale overrides og ikke til systemforvaltede CRM-/Service Bus-kerneværdier.
- `VERIFYCRM` er forbindelsestest; `DRYRUN` er teknisk flow uden ekstern effekt; `RUN` er den mode hvor write/publicering kan forekomme.
