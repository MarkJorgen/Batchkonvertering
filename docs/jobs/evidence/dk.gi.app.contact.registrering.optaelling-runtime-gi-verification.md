# GI runtime verification – dk.gi.app.contact.registrering.optaelling

## Status
- Ikke genkørt på den endelige succesfulde build i denne leverance endnu.

## Bemærkning
Tidligere top-level scanning uden `dk.gi*.dll` er ikke alene tilstrækkelig som close-out-evidens.
Denne fil er lagt ind som målplacering for den aktuelle succesfulde build.

## Sådan køres scriptet
```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Verify-GiAssemblies.ps1 `
  -OutputDirectory "<sti til jobets bin\Debug\net48>" `
  -MarkdownOut "docs\jobs\evidence\dk.gi.app.contact.registrering.optaelling-runtime-gi-verification.md"
```
