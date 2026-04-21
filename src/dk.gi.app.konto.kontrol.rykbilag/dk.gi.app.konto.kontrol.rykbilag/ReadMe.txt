/// <remarks>
///     $Workfile: ReadMe.txt
///   Description: Dette er ReadMe.txt filen som beskriver projektet.
///               
///                Programmet finde kontroller der skal rykkes og lægger job ind i Azure
///                job i Azure bevirker at Azure programmet dk.gi.asbq.konto.kontrol.rykbilag bliver startet via trigger
///
///                Husk at StrongkeyGI.snk, Program.template.cs, ReadMeTemplate.txt filerne skal sættes til "Build Action=None"
///                Husk projekt properties, filen StrongkeyGI.snk bruges til signering.
///
///        Author: GI
///       Created: 2022 07 22
///     Copyright: © 2023 Grundejernes Investeringsfond
///
///	   Build af dette projekt i visual studio kræver et antal NuGet pakker. Se i packages.config
/// =============================================================================================================================
/// YYYY MM DD INIT History
/// -----------------------------------------------------------------------------------------------------------------------------
/// 2025 01 08 RCL  Batch did not work anymore on server update to newest nuget
/// 2024 07 08 RCL  Format amount change
/// 2024 04 15 JMW  Opdateret alle nuget pakker
/// 2023 09 07 JMW  Program rettet, nu hentes de relevente kontroller og for hver der skal rykkes oprettes et job i Azure
/// 2023 02 28 RCL  Fejlrettelse validering kørte ikke
/// 2023 01 31 RCL  Updated to NET 4.8 and converted to request/response, new console, new gi.lib etc. For tst/prod
/// 2022 07 22 RCL  Oprettet og lagt i tst/prod
/// </remarks>
-->