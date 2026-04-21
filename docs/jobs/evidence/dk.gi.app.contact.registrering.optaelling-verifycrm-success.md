# VERIFYCRM evidence – dk.gi.app.contact.registrering.optaelling

## Resultat
- Status: PASS
- Mode: VERIFYCRM
- Dato: 2026-04-16
- Kilde: lokal kørsel rapporteret i projektchatten

## Centrale observationer
- `CrmAuthorityMode=TenantBase` blev anvendt korrekt
- startup-diagnostik viste alle CRM-settings som `FOUND`
- legacy-kompatibel secret-dekryptering blev anvendt:
  - `legacy decrypt applied=Ja`
  - `effective length=40`
- Dataverse-forbindelsen blev valideret succesfuldt

## Centrale loglinjer
- `Dataverse-forbindelse valideret i VERIFYCRM-mode.`
- `VERIFYCRM gennemført via local dataverse sdk. Mode=VERIFYCRM. CRM-forbindelse valideret. Der blev ikke udført workflow-kald i VERIFYCRM-mode.`

## Bemærkning
Denne evidens beviser, at den nye GI-frie auth-/connect-vej kan etablere Dataverse-forbindelse i pilotjobbet.
