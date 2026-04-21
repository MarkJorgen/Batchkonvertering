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
///
///        Author: Grundejernes Investringsfond (GI)
///       Created: 2023 04 14
///     Copyright: © 2022 Grundejernes Investeringsfond
///
///	   Build af dette projekt i visual studio kræver et antal NuGet pakker. Se i packages.config
/// =============================================================================================================================
/// YYYY MM DD INIT History
/// -----------------------------------------------------------------------------------------------------------------------------
/// 2025 01 16 JMW  Update Nuget Packages. (Tilpasset CRM i Skyen)
/// 2024 01 11 RCL  Changed to write to sb queue
/// 2023 04 20 RCL  LukSag - Close text changed
/// 2023 04 14 RCL  Oprettet. Danner rykker1 og rykker2 ved at trække på kø 2. Der oprettes en opgave med dokumenter og dokumenter sendes digitalt.
/// </remarks>
-->