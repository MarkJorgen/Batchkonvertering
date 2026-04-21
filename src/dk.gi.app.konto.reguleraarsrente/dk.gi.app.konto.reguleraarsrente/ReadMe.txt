/// <remarks>
///     $Workfile: ReadMe.txt $
///   Description: Dette er ReadMe.txt filen som beskriver projektet
///				   Dette projekt batch job bruges til at checke ap_finanssaldo for negativ årsrente som så reguleres.
///                Benytter .Net Framework 4.8
///                Henter/retter data fra CRM via.
///        
///        Author: GI
///       Created: 2020 04 15 
///     Copyright: © 2020 Grundejernes Investeringsfond
///
/// =============================================================================================================================
/// YYYY MM DD INIT History
/// -----------------------------------------------------------------------------------------------------------------------------
/// 2023 01 25 RCL  Updated converted to request/response, new console, new data.lib etc. For tst/prod
/// 2022 10 28 JMW  Opgraderet til FW. 4.8 samt nye nuget pakker.
/// 2022 10 28 JMW  Der var fejl i Program, men p.g.a. manglende Request kan man ikke se hvad fejlen er, Tilføjer korrekt fejhåndtering.
/// 2022 03 16 JMW  Opgraderet til Framework 4.6.2 og CRM 9.0
/// 2021 10 25 RCL  Ændret regel for OpdaterNegativArsrente i finanssaldomanager  
/// 2021 09 22 RCL  Konverteret til ny dk.gi.app.console.template
/// 2020 04 22 RCL  Første version af dk.gi.crm.app.konto.reguleraarsrente 
