/// <remarks>
///     $Workfile: ReadMe.txt $
///   Description: Dette er ReadMe.txt filen som beskriver projektet.
///                Dette program indeholder et antal kørsler for Anonymisering som kan/skal startes fra et kommandoprompt.
///
///
///     Namespace: dk.gi.crm.app.anonymisering
///        Author: JMW
///       Created: 2018 Aug
///     Copyright: © 2018 Grundejernes Investeringsfond
///
///	   Build i visual studio kræver et antal NuGet pakker. Se i packages.config
///        System.Web.Services
///        microsoft.crm.sdk
///        microsoft.crm.sdktypeproxy
/// =============================================================================================================================
/// YYYY MM DD INIT History
/// ---- -- -- ---- -------------------------------------------------------------------------------------------------------------
/// 2025 01 28 JMW  Opdateret Nuget pakker, Anonymisering Kontaktperson fejler, slettet felt var angivet i fetch
/// 2023 04 11 RCL  Updated to new bll etc. to correct error for removed entity Ap_slettedenoter
/// 2022 11 15 RCL  Updated to net 4.8 and 4.8 bll code etc.
/// 2022 03 09 JMW  Opdateret NuGet pakker for CRM 9 samt E-Mail via Graph i stedet for ews
/// 2022 01 19 RCL  Parat til test når ny tst/dev 9.0 server er på plads.
/// 2022 01 19 RCL  Opdateret forretning 2022.01.18 for at få ændret anonymiseringsregler for kontaktperson og ejendom med.
/// 2022 01 19 RCL  Opdateret Console skabelon til sidst nye. Husk ved opdatering i dev osv. at forbindelse er ændret til CrmConnectionTemplate og organization tilføjet.
/// 20210830 JMW Opdateret Console skabelon til sidst nye.
/// 20210615 ROB Fixet password til app.config,, rettet til lidt mere template, lavet log om fra rollingfile til single file, tilføjet send email ved fejl.
/// 20200622 RCL dk.gi.crm.faelles.forretning 2021.6.22.628 og dk.gi.crm.data 2021.6.22.618 - Optimering af ejendom anonymisering
/// 20210615 ROB Opdateret console.template til 2021.5.19.808, bruger nu krypteret password.
/// 20200326 RCL dk.gi.crm.faelles.forretning 2021.3.26.1300 og dk.gi.crm.data 2021.3.26.1233
/// 20200322 RCL Opdateret med ny forretning for at rette op på fejl i Anonymiser kontakt
/// 20200309 RCL FindOgAnonymiserVedligeholdRequest udvidet med dato for AfleveretRigsarkivet
/// 20201129 ANJ Opdateret med seneste ændringer på anonymisering på ejendom og kontakter
/// 20200921 ANJ Oprydning af NuGets iforhold til ny struktur, samt mindre rettelser sammen med JMW
/// 20201028 JMW Opdateret Data Assembly, en del problemer med at koden crasher, primært fordi der dannes mange tusinde forbindelser til CRM, det kan systemet ikke håndtere.
/// 20200515 JMW Lån Manager: SletLaanRelateredeData rettet. Opdater kun lån hvis felter er ændret
/// 20200514 JMW Tilføjet NuGet dk.gi.ews.outlook.library, som kan sende emails. (Send en email ved fejl!!)
/// 20200514 JMW Opdateret faelles.forretning NuGet pakke. (Rettet namespaces)
/// 20200228 ANJ Tilføjet Kontakt metode
/// 20200228 ANJ Tilføjet Ejendoms metode
/// 20200211 JMW Rettet Teknikerservice, status blev ikke sat til Anonymiseret. Hack på ExplicitChangedOndate havde også lille fejl som er rettet i forretning.
/// 20200206 JMW Mindre rettelser samt build.
/// 20191118 JMW Opdateret Nuget pakker(Udviklings build af data og forretning). Stadig lille fejl i sletning af Aktiviteter.
/// 20191029 JMW Opdateret Assemblies med rettelser til Treklip, Registrering, Vedligehold og Teknikerservice (Lagt i Udvikling)
/// 20190417 ANJ Lavet nyt brach for igangsætning af opgave fra Henning omkring anonymisering af følgende entiteter: Ejendom, Kontaktperson, Kontoejer, Ultimativejer, Kontrol, Lån
/// 20180816 JMW Start på opgave med at lave Anonymisering på entiteter: Vedligehold, Registrering, TreKlip og Teknikerservice
///          Test:
///           -MODE=REGISTRERING -CRMSERVER=crm.udv.gi.dk -USERNAME=csu@gisb.dk -PASSWORD=Password-1
/// </remarks>
-->