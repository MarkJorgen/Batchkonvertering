/// <remarks>
///     $Workfile: ReadMe.txt $
///   Description: Checker om CRM finanspostering(er) er i sync med økonomisystem.
///                Er det ikke tilfældet forsøges udligning af så mange posteringer som muligt.
///                Der sendes en mail.
///                Der dannes en fil med posteringer der ikke kan udlignes som lægges på fælles drev.
///                I app.config er findes output sti der skal ændres for TEST/PROD - fælles drev.
///                
///
///	   Build i visual studio kræver at der er referencer til følgende
///    Pakker:
///            - dk.gi.crm.data
///            - dk.gi.crm.konto.forretning
/// =============================================================================================================================
/// YYYY MM DD INIT History
/// -----------------------------------------------------------------------------------------------------------------------------
/// 2023 02 08 RCL  Check/Create dir FinansAfstemning
/// 2023 01 30 RCL  Changed write of AfstemningsInfo.json to FinansAfstemning path
/// 2023 01 23 RCL  Updated to 4.8 NET, converted to request/response, new console, new bll.lib etc. For tst/prod
/// 2022 08 02 RCL  Opdateret til ny forretning. Ændret bc til BcOAuthClientSecretContext. Lagt i tst/prod
/// 2022 03 14 JMW  Opdateret NuGet pakker. CRM 9 og .Net Framework 4.6.2
/// 2021 09 30 JMW  Program meldte fejl selvom det var gået ok. Rettet + Rebuild
/// 2021 09 06 JMW  Opdateret console template
/// 2021 02 08 ROB  Opdateret med nyeste assemblies: konto.forretning 2021.1.19.821 & dertil gi.crm.data: 2021.1.13.744
/// 2021 01 18 ROB  Tilføjet krypteret password og andre vars i app.config, tilføjet email ved fejl, opdateret med nyeste assemblies.
/// 2020 09 17 RCL  Luk og opdateret til nyeste forretning
/// 2020 09 15 RCL  Opdatering af navn i integrationslog med crm/øko beløb
/// 2020 07 24 RCL  Ny klassse til udligning af åbne posteriner 
/// 2020 07 24 RCL  Fjernet AX og rettet integrationslog så flere kan modtage mail 
/// 2020 04 03 RCL  Tilføjet BC til erstatning AX når FinansSystem forskellig fra AX
/// 2020 03 05 RCL  Tilføjet tilbageholdt beløb til kontoindestående
/// 2019 12 16 RCL  Første version af job til afstemning af finansposteringer
