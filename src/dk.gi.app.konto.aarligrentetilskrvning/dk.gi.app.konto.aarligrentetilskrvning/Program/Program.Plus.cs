/// <summary>
/// Koden her er den del af GI Skabelon til GIConsoleApp som er en hjælpeklasse for at gøre det lettere og hurtigere at opdatere skabelon kildekoden
/// 
/// Version: 2022 03 10 
/// Sidste ændring: Tilføjet AppStatus
/// 
/// Når du opretter en ny applikation er det tanken at denne Program.Plus-template.cs kopieres til app Program.Plus.cs (Og sættes til Compile)
/// Når du opgraderer NuGet Pakken "dk.gi.app.console.template.GiNugetSrc", så må du manuelt opdatere denne kode så den får de rettelse som efterfølgende er lavet i skabelonen
/// - Efterfølgende lægger du din kode ind i GIConsoleApp Start metoden. 
/// </summary>

// GI
using dk.gi.app;

//namespace dk.gi.app.console.template
namespace dk.gi.app.konto.aarligrentetilskrvning
{
    /// <summary>
    /// Dette er en "partial" del af Program
    /// - her befinder sig den del af koden som der oftest skal ændres.
    /// </summary>
    public partial class Program
    {
        //******************************************************************************
        // Tilføj navne på parametre som skal indlæses fra appconfig eller Environment (eller fra programparametre(args), navne på args er ikke krævede de er inkluderet automatisk)
        //******************************************************************************
        internal static string[] ekstraParametre = new string[] { };

        /// <summary>
        /// Definer om der dannes et ekstra directory for Mode parameter
        /// </summary>
        /// <param name="_configure">reference til den GIAppConfigure som oprettes i main</param>
        //internal static void SetTraceMode(ref GIAppConfigure _configure)
        //{
        //    //_configure.traceSetModeAsFolder = true; // Hvis denne er sat, så dannes der en ekstra folder under trace folder (med navn som mode)
        //}

        /// <summary>
        /// Her tilføjes navne på parametre som skal være til stede i programmet for at det kan køre
        /// - parametre som kun kræves i nogle tilfælde skal stadig valideres særskildt
        /// </summary>
        /// <param name="_configure">reference til den GIAppConfigure som oprettes i main</param>
        internal static void SetKraevedeParametre(ref GIAppConfigure _configure)
        {
            // Tilføj dine krævede parametre til array som bruges i ContainsAll.  Bemærk CRM, Email og MSMQ valideres efter denne if
            _configure.requiredParametre = new string[] { }; // Her tilføj parameter navne til denne
        }

        /// <summary>
        /// Her skal du åbne for de linjer med validering som skal bruges i dit program
        /// </summary>
        /// <param name="_configure">reference til den GIAppConfigure som oprettes i main</param>
        /// <returns></returns>
        internal static AppStatus.StateCode ValidateKraevedeParametre(ref GIAppConfigure _configure)
        {
            // Validering af parametre
            if (_configure.ValidateRequired() == false)
                return AppStatus.StateCode.AppRequiredParmsMissing;

            ////****************************************************************************************************************************************************************************
            //// Åben for disse 2 linjer hvis du skal have forbindelse til CRM, 
            //// - og husk at fjerne kommentarlinjer i funktionen OpretCrmConnection hvis du skal bruge CRM (Den er i GIConsoleApp-CRM.cs)
            ////****************************************************************************************************************************************************************************
            if (_configure.ValidateCrmConfiguration() == false)
                return AppStatus.StateCode.AppRequiredCrmParmsMissing;

            ////****************************************************************************************************************************************************************************
            //// Åben for disse 2 linjer hvis du skal sende Emails og vil have valideret at nødvendige parametre er til stede
            ////****************************************************************************************************************************************************************************
            if (_configure.ValidateEmailConfiguration() == false)
                return AppStatus.StateCode.AppRequiredEmailParmsMissing;

            ////****************************************************************************************************************************************************************************
            //// Åben for disse 2 linjer hvis du skal forbinde til MSMQ, hvis app er startet fra MSMQ trigger (i GIConsoleApp-MSMQ.cs) så husk at
            //// - fjerne kommentarlinjer i funktionen HentMSMQFraIndkomneKoe, FlytMessageToAfsluttet og FlytMessageToFejl
            ////****************************************************************************************************************************************************************************
            //if (_configure.ValidateMSMQConfiguration() == false)
            //    return AppStatus.StateCode.AppRequiredMsmqParmsMissing;

            return AppStatus.StateCode.OK;  // Alt ok, retuner 0
        }

        ///************************************************************************************************************************************
        /// <summary>
        /// Her valideres om app allerede kører i forvejen, og dermed kan vi stoppe den hvis vi ikke vil have flere instanser kørende samtidigt
        /// </summary>
        /// <returns>Default false, true hvis den kører i forvejen</returns>
        ///************************************************************************************************************************************
        internal static bool IsApplicationActive()
        {
            ////****************************************************************************************************************************************************************************
            //// Åben for disse 6 linjer hvis du skal sikre at dit program kun kører i en instans
            ////****************************************************************************************************************************************************************************
            //if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length > 1)
            //{
            //    WriteLineTempTraceLog($"Der kører allerede en {appName} kørsel. Vi stopper...");
            //    WriteLineTempTraceLog($"[{appName}]Program.Main stop:{System.DateTime.Now.ToLongTimeString()}");
            //    return true;
            //}
            return false;
        }

        /// <summary>
        /// Her valideres om app allerede kører i forvejen, og dermed kan vi forsinke den en lille smule hvis vi ikke vil have flere instanser kørende samtidigt
        /// - i første omgang blev det lavet til flettebrev som gerne må køre med flere instanser, men for at få breve vedhæftet samme aktiviteter, skal den første nå at oprette før nr 2 kommer til
        /// </summary>
        /// <param name="SecondsToSleep">Brug f.eks. 5</param>
        /// <param name="MaxWaitCount">Brug f.eks 12, det vil give et minut</param>
        /// <returns>0 Hvis klar til at køre, != 0 hvis der stadig er flere instanser</returns>
        internal static AppStatus.StateCode WaitIfApplicationActive(int SecondsToSleep, int MaxWaitCount)
        {
            AppStatus.StateCode result = AppStatus.StateCode.OK; // 0 = Alt OK

            ////****************************************************************************************************************************************************************************
            //// Åben for alle disse linjer i While hvis du skal sikre at dit program kun kører i en instans
            ////****************************************************************************************************************************************************************************
            //while (MaxWaitCount > 0)
            //{
            //    // antal af kørende instanser
            //    int antal = System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length;
            //    // Hvis der kun kører denne ene instans, så er det ok
            //    if (antal == 1)
            //    {
            //        // Der er kun den ene, så vi kan afslutte while
            //        break;
            //    }
            //    else
            //    {
            //        // Hvis vi har nået max iterationer, så afslut også while
            //        if (MaxWaitCount <= 0)
            //        {
            //            result = antal;
            //            break;
            //        }
            //    }
            //
            //    WriteLineTempTraceLog($"Der kører allerede en {appName} kørsel. Vi venter lige lidt...");
            //
            //    // Vent x sekunder før vi prøver igen
            //    System.Threading.Thread.Sleep(1000 * SecondsToSleep);
            //
            //    WriteLineTempTraceLog($"Vi tæller ned fra {MaxWaitCount}");
            //    // Træk 1 fra antallet af gange som vi vil vente
            //    MaxWaitCount--;
            //}

            // Retuner resultat
            return result;
        }
    }
}