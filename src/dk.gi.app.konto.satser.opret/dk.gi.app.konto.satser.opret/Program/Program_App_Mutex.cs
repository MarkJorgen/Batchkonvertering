using Microsoft.Extensions.Logging;
/// <summary>
/// Koden her er den del af klassen GIConsoleApp (Partial)
/// 
/// Version: 2022 09 09
/// Sidste ændring: Tilpasset til Extensions Logging
/// 
/// Dette tilføjer en Mutex funktion GIConsoleApp
/// - Formålet er at gøre det muligt at vente på anden kode der kører som benytter samme ressurser som denne
/// 
/// </summary>
//
using System;
using System.Threading;

// 

//namespace dk.gi.app.console.template
namespace dk.gi.app.konto.satser.opret
{
/// <summary>
    /// Din kode skal tilføjes i Metoden Start her i dette objekt
    /// </summary>
    public partial class GIConsoleApp
    {

        /// <summary>
        /// Mutex håndtering
        /// En Mutex kan bruges til at styre at kun en instans af programmet afvikler og at de øvrige venter til det bliver deres tur
        /// </summary>
        /// <param name="callback">Funktion som kaldes når den aktuelle tråd får lov til at køre</param>
        /// <param name="uniqueMutexName">Navn på program/rutine som skal afviles Synkront</param>
        /// <param name="IfTimeoutRunAnyway">Når tiden udløber og kode stadig ikke har fået lov og køre, skal den så starte alligevel</param>
        /// <returns>Status på afviklingen af kode</returns>
        internal AppStatus.StateCode RunOrWaitForGoSignal(Func<AppStatus.StateCode> callback, string uniqueMutexName, bool IfTimeoutRunAnyway)
        {
            AppStatus.StateCode result = AppStatus.StateCode.OK;
            int SecondsToSleep = 30;
            int MaxWaitCount = 5;
            if (appConfig.ContainsAll(new string[] { "SecondsToSleep", "MaxWaitCount" }) == true)
            {
                SecondsToSleep = int.Parse(appConfig["SecondsToSleep"]);
                MaxWaitCount = int.Parse(appConfig["MaxWaitCount"]);
            }

            Trace.LogDebug("RunOrWaitForGoSignal: Wants to Enter Critical Section for processing");
            bool gotMutex = false;
            try
            {
                Mutex mutex = null;
                Trace.LogDebug($"Udfør using på Mutex");
                // Named Mutexes are available computer-wide. Use a unique name.
                // Initializes a new instance of the Mutex class with a Boolean value that indicates whether the calling thread should have initial ownership of the mutex,
                // and a string that is the name of the mutex.
                // Her false, vi har ikke brug for at det kun er dette program som har retten til at 'ReleaseMutex' igen
                using (mutex = new Mutex(false, uniqueMutexName))
                {

                    for (int i = 0; i < MaxWaitCount; i++)
                    {
                        Trace.LogDebug("WaitOne for mutex: " + uniqueMutexName);
                        //Blocks the current thread until the current WaitOne method receives a signal.  
                        //Wait until it is safe to enter. 
                        if (mutex.WaitOne(1000 * SecondsToSleep))
                        {
                            gotMutex = true;
                            try
                            {
                                Trace.LogDebug("Success: " + uniqueMutexName + " callback is Processing now");
                                result = callback();
                                Trace.LogDebug("Final: " + uniqueMutexName + " callback has Completed its task");
                            }
                            finally
                            {
                            }
                            i = MaxWaitCount;
                        }
                        else
                        {
                            Trace.LogDebug("Waiting for mutex: " + uniqueMutexName);
                            Thread.Sleep(500);
                        }
                    }
                    //Call the ReleaseMutex method to unblock so that other threads that are trying to gain ownership of the mutex can enter  
                    if (gotMutex == true)
                        mutex.ReleaseMutex();
                    else
                        Trace.LogDebug("Failure: " + uniqueMutexName + " no mutex within timelimit");
                }
            }
            catch (System.Threading.AbandonedMutexException aex)
            {
                if (IfTimeoutRunAnyway == true)
                {
                    Trace.LogWarning("AbandonedMutexException i RunOrWaitForGoSignal: " + aex.Message + " 'run annyway' er true!");
                    Trace.LogWarning("Callback metode kaldes, program bliver muligvis gennemført korrekt!");
                }
                else
                    Trace.LogError("AbandonedMutexException in RunOrWaitForGoSignal: " + aex.Message);

            }
            catch (Exception ex)
            {
                Trace.LogError(ex, "Der er sket en Exception");
            }
            finally
            {

                // Hvis ikke vi har en mutex, så er callback heller ikke kaldt og vores kode ikke kørt
                if (gotMutex == false)
                {
                    // Hvis så switch er sat for at program skal køre alligvel så gør det
                    if (IfTimeoutRunAnyway == true)
                    {
                        Trace.LogDebug("No mutex, but IfTimeoutRunAnyway is true, so run callback");
                        try
                        {
                            Trace.LogDebug("Success: " + uniqueMutexName + " callback is Processing now");
                            result = callback();
                            Trace.LogDebug("Final: " + uniqueMutexName + " callback has Completed its task");
                        }
                        finally
                        {
                            Trace.LogDebug("finish run callback");
                        }
                    }
                    else
                        // vores callback kode er ikke kørt, så derfor sæt fejl
                        result = AppStatus.StateCode.AppUventetFejlIProgramKode;
                }
            }

            // 2022 07 04 RCL Hvis vejl så skriv til log
            if (result != AppStatus.StateCode.OK)
            {
                Trace.LogError($"RunOrWaitForGoSignal slut: {result}");
            }
            else
            {
                Trace.LogInformation($"RunOrWaitForGoSignal slut: {result}");
            }

            return result;
        }
    }
}