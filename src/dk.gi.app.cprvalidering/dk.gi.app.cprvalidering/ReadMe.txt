/// <remarks>
///     $Workfile: ReadMe.txt
///   Description: Dette er ReadMe.txt filen som beskriver projektet.
///               
///                Kopier denne til Roden af dit projekt og omdøb til ReadMe.txt, derefter 
///                indtast en sigende beskrivelse af formålet af applikationen.
/// 
///                Kopier program.template.cs til program.cs, og fjer kommentar fra linjer med CRM, EMail og MSMQ (Hvis du skal bruge det)
///
///                Kopier GIConsoleApp-CRM.template.cs til GIConsoleApp-CRM.cs, og fjer kommentar fra linjer i OpretCrmConnection (Hvis du skal bruge det)
///                Kopier GIConsoleApp-MSMQ.template.cs til GIConsoleApp-MSMQ.cs, og fjer kommentar fra linjer i HentMSMQFraIndkomneKoe,FlytMessageToAfsluttet og FlytMessageToFejl (Hvis du skal bruge det)
///                Kopier GIConsoleApp-SendEmail.template.cs til GIConsoleApp-SendEmail.cs, og fjer kommentar fra linjer i SendEmail (Hvis du skal bruge det)
///             
///                Kopier GIConsoleApp.template.cs til GIConsoleApp.cs, og tilføj din kode
///
///                Husk at StrongkeyGI.snk, Program.template.cs, ReadMeTemplate.txt filerne skal sættes til "Build Action=None"
///                Husk projekt properties, filen StrongkeyGI.snk bruges til signering.
///
///                Dette program afvikler dk.gi.app.msmq.faelles.cprvalidering.
///
///     Namespace: 
///        Author: GI
///       Created: 2022 07 22
///     Copyright: © 2022 Grundejernes Investeringsfond
///
///	   Build af dette projekt i visual studio kræver et antal NuGet pakker. Se i packages.config
/// =============================================================================================================================
/// YYYY MM DD INIT History
/// -----------------------------------------------------------------------------------------------------------------------------
/// 2025 03 06 RCL  Rebuild not workning on server and removed asbqLabel
/// 2025 01 06 RCL  Rebuild not workning on server
/// 2023 05 11 JMW  Fik ikke slettet det ene parameter
/// 2023 04 05 JMW  Delt program op, denne finder dem der skal udføres CPR Chileck på, og kø programmet behandler hver enkelt
///                 StartCprValideringRequest og response udgår, der er intern kode der kan det samme. (i dk.gi.msmq namespace)
/// 2023 01 23 RCL  Updated to 4.8 NET, converted to request/response, new console, new gi.lib etc. For tst/prod
/// 2022 07 22 RCL  Oprettet og lagt i tst/prod
/// </remarks>
-->