# 0016 - Lokal Dataverse-erstatning for ContactBLL

Pilotjobbet `dk.gi.app.contact.registrering.optaelling` har erstattet det tidligere reflection-baserede GI-seam med en lokal implementation oven på Microsoft Dataverse SDK.

Beslutninger:
- `SetAfsluttedeTreklipEjerRegistreringerInaktive()` er omskrevet til direkte QueryExpression + SetStateRequest mod Dataverse
- kontaktoptællingsflowet er omskrevet til direkte CRM-queries og kontaktopdateringer uden `ContactBLL`
- `LegacyGiAssemblyDirectory` udgår som aktiv runtime-setting
- runtime i `RUN` og `VERIFYCRM` afhænger ikke længere af `dk.gi.crm.lib` / `dk.gi.crm.data.lib`

Konsekvens:
- reflection-seamet er fjernet som aktivt runtime-spor
- pilotjobbet er tættere på reel GI-uafhængighed i drift
- næste step er lokal validering af `RUN` samt opdateret runtime-verifikation
