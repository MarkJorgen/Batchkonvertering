/// <remarks>
///     $Workfile: ReadMe.txt
///   Description: Dette er ReadMe.txt filen som beskriver dk.gi.app.console.template projektet.
///                Et template projekt som kan benyttes som en let og hurtig vej til et nyt consol program.
///                Ud af boksen gives her et mindre antal standard funktioner som gør det let at komme i luften med et nyt console projekt.
///
///                - Tilføj NuGet pakke dk.gi.library eller dk.gi.GINugetSrc for at få adgang til standard GI funktionalitet og Extendings
///                - Tilføj NuGet pakke dk.gi.crm.GiNugetSrc eller dk.gi.crm.GiNugetSrc for at få let adgang til CRM standard funktioner
///
///                - Tilføj NuGet pakke Serilog.Settings.Configuration til Serilog ved hjælp af JSon config fil           
///                - Tilføj pakke Serilog.Settings.AppSettings til Serilog ved hjælp af App.config filen
///
///                Templates: Kopier og fjern template fra navn.
///						Program.template.cs..................... Denne indeholder en standardiseret program.cs, ret kun de nødvendige linjer i denne
///                     Program.Plus_template.cs................ Her retter du navne på argumenter til programmet.
///                     Program_App_template.cs................. Tilpas parameter og opstart i denne
///                     Program_App_callback.cs................. Tilpas og tilføj mere kode til denne
///                     Program_App_ctor_template.cs............ Ret ikke i denne
///                     Program_App_Mutex_template.cs........... Første version af denne, skal på sigt bruges til styring så apps kan gøres synkrone i kørsel
///                     Program_App_Plus_CRM_template.cs........ Skal du bruge CRM så fjern kommentar i denne
///                     Program_App_Plus_MSMQ_template.cs....... Skal du bruge MSMQ så fjern kommentar i denne
///                     Program_App_Plus_SendEmail_template.cs.. Skal du bruge Mail så skal du blot sikre de korrekte app settings til Azure Graph E-Mail
///
/// =============================================================================================================================
/// YYYY MM DD INIT History
/// -----------------------------------------------------------------------------------------------------------------------------
/// 2023 01 19 JMW  Rettet fejl, Namespace som en del af LogPath var faldet tidligere
/// 2023 01 17 JMW  Rettet fejl, indlæsning af LogPath var faldet ud i sidste rettelse
/// 2022 12 19 JMW  Rettet brugen af Status kode så vi får bedre fejl igennem
/// 2022 11 30 JMW  Rettet fejl i Template
/// 2022 11 29 JMW  Mindre tilpasning omkring logning.
/// 2022 10 26 JMW  Tilføjet Parameter til CRM forbindelse, samt rettet oprettelse af CrmContext
/// 2022 10 24 JMW  Flyttet call-back funktion til egen kildekode fil
/// 2022 10 05 JMW  Rettet lidt bedre tekst og bedre oversigt
/// 2022 09 26 JMW  Rettet fejl, fik ikke dannet Logging Factory
/// 2022 09 12 JMW  Opgraderet til Framework 4.8
/// 2022 07 15 JMW  Rettet i Mutex kode under finally, den skrev altid fejl til log, det skal den jo ikke
/// 2022 06 13 JMW  Rettet Mutex kode.
/// 2022 03 29 JMW  Opgraderet: Mindste krav af dk.gi.library er version 2022.3.17.2000
/// 2022 03 14 JMW  Der var lidt for mange referencer til Serilog
/// 2022 03 11 JMW  Rettet Program Main thread så det kører dansk.
/// 2022 03 11 JMW  Rettet fejltekster i AppStatus
/// 2022 03 10 JMW  Tilføjet AppStatus klasse som bedre kan styre parametre til programmet (Rettet fejl i AppStatus)
/// 2022 03 08 JMW  Der var lige den lille krølle at Temp Trace fil ikke slettes ved når der testet på om program kører i forvejen.
/// 2022 03 03 JMW  Implementering af Email afsendelse fra Console app sker ved hjælp af Azure Graph
/// 2022 01 19 JMW  Opgraderet til 4.6.2
/// 2022 01 19 JMW  Rettet trace fil tidspunkt for filnavn, samt tilføjet en WaitIfApplicationActive som kan vente lidt når app kører i forvejen
/// 2022 01 13 JMW  Tilføjet 2 cifre mere i logfilnavn.
/// 2021 11 12 JMW  Havde ikke fået rettet namespace rigtigt. 
/// 2021 11 01 JMW  Tilføjet CRM Timeout parameter. Build + Nuget pakke version 2021.11.11.
/// 2021 11 01 JMW  Build + Nuget pakke version 2021.11.11.1232
/// 2021 11 01 JMW  Tilføje Ny klasse Specialtegn til at pakke applikation's parametre(args) ind og ud igen
/// 2021 10 25 JMW  Tilføje parameter CrmOrganisationName
/// 2021 10 14 JMW  Kode som ofte skal rettes er nu flyttet ud i program.Plus.cs
/// 2021 09 03 JMW  Fejl i GIAppConfigure - gav altid invalid i parametre hvis ikke der var nogen program args.
/// 2021 08 31 JMW  Fejl i ContainsAll - Flyttet kryptering af email password :-)
/// 2021 08 30 JMW  Fejl i konfiguration af ekstraparametre. Samt lave mulighed for Subfolder på sti til trace.
/// 2021 08 25 JMW  Manglede linje med forbindelse til CRM
/// 2021 08 23 JMW  Flyttet definition af parametre til GIAppConfigure, ekstraParametre og dineKrævedeParametre
/// 2021 07 29 JMW  Program.cs er minimeret, Til er så kommet GIConsoleApp med tilhørende templates.
/// 2021 07 28 JMW  Tilføjet GIAppConfigure.cs som erstatter indlæsning af argumenter i Argumentparser.cs
/// 2021 07 27 JMW  Påbegyndt flytning af configuration og opstart til Startup.cs
/// 2021 06 28 JMW  Sikre at mode bliver skrevet til trace :-)
/// 2021 05 18 JMW  Mindre rettelser af fejl i variabel navne m.m.
/// 2021 04 22 JMW  Rettet Tidsstempel på logfil, 24 timers i stedet for 12. Build + Nuget. Version dk.gi.app.console.template.GiNugetSrc.2021.4.22.537.nupkg
/// 2021 03 03 JMW  Build + NuGet version 2021.3.3.651
/// 2020 11 24 JMW  Build + NuGet version 2020.11.24.0620
/// 2020 11 23 JMW  Der var et problem med at overføre parametre til en app, hvor tegnet '=' indgår som en del af parameter f.eks: -password=QQBwAGIAYgBmAH4AYwB1ADwAIAA=
/// 2020 10 21 JMW  Fjernet reference til dk.gi.library, og i stedet tilføjet en kommentar om at tilføje NuGet pakke dk.gi.library eller dk.gi.GINugetSrc
/// 2020 09 03 JMW  Opgraderet reference til dk.gi til version 2020.8.31.856
/// 2020 08 27 JMW  Mindre rettelser. Kommenteret kode til CRM brug ud, så det stadig ligger der og er nemt at tage i brug.
/// ---- -- -- ---  Oprettet.
/// </remarks>
-->