/// <summary>
/// Koden her er den del af klassen GIAppConfigure
/// Version: 2023 01 09 JMW Tilføjet parametre SecondsToSleep og MaxWaitCount der bruges til Mutex
/// Version: 2022 03 04 JMW Tilføjet parametre for at ændre email afsendelse fra EWS og til at bruge Graph i stedet
/// Version: 2022 01 19 JMW Rettet tidsstempel i logfil navn
/// Version: 2022 01 13 JMW Tilføjet 2 cifre mere i logfil navn
/// Version: 2021 11 11 JMW TimeOut parameter til CRM var faldet ud af konfiguration
/// Version: 2021 09 16 JMW Rettet læsning af msgID parameter
/// Version: 2021 10 28 JMW Tilføjet mulighed for at indlæse parametre med special tegn i via funktionerne i klassen Specialtegn (Den kræver en opdateret version af gi.dk)
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Tilføj Nuget pakke dk.gi.library eller dk.gi.GINugetSrc - for at få adgang til trace
//using dk.gi.trace;

namespace dk.gi.app
{
    /// <summary>
    /// Objekt til indlæsning af parameter til programmet
    /// 1) Først hentes 
    /// </summary>
    internal class GIAppConfigure
    {
        #region Faste navne
        // Ret ikke i disse
        internal static readonly string APPNAME = "appName";
        internal static readonly string MODE = "Mode";
        internal static readonly string LOGPATH = "LogPath";
        // i en periode vil vi indlæse string fra CrmConnectionString til variabel CrmConnectionTemplate, det giver os nogen tid til at ændre variabel navn diverse steder
        internal static readonly string CRMCONNECTIONSTRING = "CrmConnectionString";
        internal static readonly string CRMCONNECTIONSTEMPLATE = "CrmConnectionTemplate";
        internal static readonly string CRMSERVERNAME = "CrmServerName";
        internal static readonly string CRMORGANISATIONNAME = "CrmOrganisationName";
        internal static readonly string CRMUSERNAME = "CrmUserName";
        internal static readonly string CRMUSERPASSWORD = "CrmUserPassword";
        internal static readonly string GIMSMQSERVER = "gimsmqServer";
        internal static readonly string TIMEOUTMINUTTER = "TimeOutMinutter";
        internal static readonly string REUSESERVICECLIENT = "reuseserviceclient";
        internal static readonly string MSGID = "msgID";
        // Email
        internal static readonly string AZUREEMAILURL = "Azure.Email.Url";
        internal static readonly string AZUREEMAILTENANTID = "Azure.Email.TenantID";
        internal static readonly string AZUREEMAILCLIENTID = "Azure.Email.ClientID";
        internal static readonly string AZUREEMAILCLIENTSECRET = "Azure.Email.ClientSecret";
        internal static readonly string AZUREEMAILAFSENDEREMAIL = "Azure.Email.AfsenderEmail";
        // Modtager liste/array
        internal static readonly string MODTAGEREEMAIL = "modtagereEmail";
        // Mutex bruger disse
        internal static readonly string SECONDSTOSLEEP = "SecondsToSleep";
        internal static readonly string MAXWAITCOUNT = "MaxWaitCount";

        #endregion

        internal string[] programargs = null;

        // Version: 2022 01 19 JMW Bruges til filnavn på log
        internal static readonly string appConfigStartTime = System.DateTime.Now.ToString("yyyyMMdd-HHmmssff");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appName">Navnet på denne app</param>
        /// <param name="args">Program parametre</param>
        /// <param name="ekstraparametre">Navne på ekstra parametre der ønskes indlæst</param>
        internal GIAppConfigure(string appName, string[] args, string[] ekstraparametre)
        {
            // C# er ikke ret god til at håndtere exceptions i .ctor så derfor fanges alle fejl her
            try
            {
                this.AddConfiguration(APPNAME.ToLower(), appName); // Første parameter - bruges til Trace
                programargs = args;
                try
                {
                    // Ekstra navngivne parametre
                    if (ekstraparametre != null && ekstraparametre.Length >= 1)
                        ekstraParametre = ekstraparametre;
                    // Konfigurer
                    this.ConfigureFromAppEnvironment();
                    this.ConfigureFromAppSettings();
                    this.ConfigureFromAppArgs(programargs);
                }
                catch (Exception ex)
                {
                    // Vi har indtil videre ingen trace, så det eneste vi kan er og skrive til Console
                    Console.WriteLine("Exception i GIAppConfigure.ctor");
                    Console.WriteLine(ex.ToString());
                }
            }
            catch
            {
            }
        }

        #region Liste med alle parametre til programmet, samt hjælpefunktioner for at tilgå disse

        /// <summary>
        /// Tabel med de indlæste parametre
        /// </summary>
        internal SortedList<string, string> appConfigureTable { get; set; } = new SortedList<string, string>();

        /// <summary>
        /// Tilføj et parameter samt værdi til den interne liste af config settings
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        internal void AddConfiguration(string key, string value)
        {
            try
            {
                if (appConfigureTable.ContainsKey(key.ToLower()) == false)
                {
                    // add this to table
                    if (string.IsNullOrEmpty(value) == false)
                        appConfigureTable.Add(key.ToLower(), value);
                    else
                        appConfigureTable.Add(key.ToLower(), string.Empty);

                }
                else
                {
                    //substitute the value with the latest one
                    if (string.IsNullOrEmpty(value) == false)
                        appConfigureTable[key.ToLower()] = value;
                    else
                        appConfigureTable[key.ToLower()] = string.Empty;
                }
            }
            catch (ArgumentException argEx)
            {
                Console.WriteLine(argEx.Message + Environment.NewLine + argEx.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }

        /// <summary>
        /// Indeholder configuration denne key og er den forskellig fra tom streng
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal bool ContainsKey(string key)
        {
            if (appConfigureTable.ContainsKey(key.ToLower()) == true)
                if (string.IsNullOrEmpty(appConfigureTable[key.ToLower()]) == false)
                    return true;
            return false;
        }

        /// <summary>
        /// Validering af parameter navne er til stede
        /// </summary>
        /// <param name="keynames"></param>
        /// <returns></returns>
        internal bool ContainsAll(string[] keynames)
        {
            foreach (string key in keynames)
            {
                if (appConfigureTable.ContainsKey(key.ToLower()) == false)
                {
                    Console.WriteLine($"Program configuration manglede et krævet parameter:{key}");
                    // Vi retunerer så snart vi har fundet et parameter som ikke er til stede
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Indexer property
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal string this[string key] {
            get {
                if (appConfigureTable.ContainsKey(key.ToLower()) == true)
                    return appConfigureTable[key.ToLower()];
                else
                    return string.Empty;
            }
        }

        #endregion

        /// <summary>
        /// Navne på Parametre som der skal forsøges at indlæse fra AppSettings med mere
        /// </summary>
        public string[] ekstraParametre { get; internal set; } = new string[] { };

        /// <summary>
        /// Navne på Parametre som skal være til stede (Krævede)
        /// </summary>
        public string[] requiredParametre { get; internal set; } = new string[] { };

        #region Hardcoded standard GI variable som bruges i de fleste console applikationer

        /// <summary>
        /// Hardcoded property for Mode
        /// </summary>
        internal string Mode {
            get {
                if (appConfigureTable.ContainsKey(MODE.ToLower()) == true)
                    return appConfigureTable[MODE.ToLower()];
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Hardcoded property for msgId
        /// </summary>
        internal string msgIDAsBasse64String {
            get {
                if (appConfigureTable.ContainsKey(MSGID.ToLower()) == true)
                    return appConfigureTable[MSGID.ToLower()];
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Hardcoded property for msgId
        /// - Her bliver den konverteret til UniCode som er det vi bruger i koden
        /// </summary>
        internal string msgID {
            get {
                if (appConfigureTable.ContainsKey(MSGID.ToLower()) == true)
                {
                    // Undersøg om msgid indeholder en af disse 3 tegn {}\ så der den nemlig ikke base 64 encoded
                    string id = appConfigureTable[MSGID.ToLower()];
                    if (id.Contains("{") == true || id.Contains("}") == true || id.Contains("\\") == true)
                    {
                        return id;
                    }
                    else
                    {
                        byte[] data = System.Convert.FromBase64String(id);
                        Encoding unicode = Encoding.Unicode;
                        return unicode.GetString(data);
                    }
                }
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Hardcoded property for appName
        /// </summary>
        internal string appName {
            get {
                if (appConfigureTable.ContainsKey(APPNAME.ToLower()) == true)
                    return appConfigureTable[APPNAME.ToLower()];
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Hardcoded proberty for trace folder path
        /// </summary>
        internal string traceFolderPath {
            get {
                if (appConfigureTable.ContainsKey(LOGPATH.ToLower()) == true)
                {
                    string folder = appConfigureTable[LOGPATH.ToLower()];
                    if (this.traceSetModeAsFolder == true)
                    {
                        if (folder.EndsWith(@"\") == true)
                            folder += this.Mode + @"\";
                        else
                            folder += @"\" + this.Mode;

                        return folder;
                    }
                    else
                        return folder;
                }
                else
                    if (this.traceSetModeAsFolder == true)
                    return @"C:\Temp\" + this.Mode;
                else
                    return @"C:\Temp";
            }
        }

        /// <summary>
        /// Sæt denne til true hvis du vil have Mode som en subfolder på den valgte trace setting
        /// </summary>
        internal bool traceSetModeAsFolder { get; set; } = false;

        /// <summary>
        /// Sammensæt en streng med path til brug for trace
        /// </summary>
        internal string sporingsLog {
            get {
                /// Version: 2022 01 19 JMW Tilføjet appConfigStartTime
                string tracePath = this.traceFolderPath + @"\" + appConfigStartTime + "-" + this.appName + "-" + this.CrmServerName + ".log.txt";
                return tracePath;
            }
        }
        #endregion

        #region Hardcoded CRM part
        /// <summary>
        /// Hardcoded property for CrmConnectionString 
        /// - Denne udgår ersttes med CrmConnectionTemplate
        /// </summary>
        //internal string CrmConnectionString
        //{
        //    get
        //    {
        //        if (appConfigureTable.ContainsKey(CRMCONNECTIONSTRING.ToLower()) == true)
        //            return appConfigureTable[CRMCONNECTIONSTRING.ToLower()];
        //        else
        //            return string.Empty;
        //    }
        //}

        /// <summary>
        /// Hardcoded property for CrmConnectionTemplate
        /// </summary>
        internal string CrmConnectionTemplate {
            get {
                if (appConfigureTable.ContainsKey(CRMCONNECTIONSTEMPLATE.ToLower()) == true)
                    return appConfigureTable[CRMCONNECTIONSTEMPLATE.ToLower()];
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Hardcoded property for CrmServerName
        /// </summary>
        internal string CrmServerName {
            get {
                if (appConfigureTable.ContainsKey(CRMSERVERNAME.ToLower()) == true)
                    return appConfigureTable[CRMSERVERNAME.ToLower()];
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Hardcoded property for Crm Organisation
        /// </summary>
        internal string CrmOrganisationName {
            get {
                if (appConfigureTable.ContainsKey(CRMORGANISATIONNAME.ToLower()) == true)
                    return appConfigureTable[CRMORGANISATIONNAME.ToLower()];
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Hardcoded property for CrmUserName
        /// </summary>
        internal string CrmUserName {
            get {
                if (appConfigureTable.ContainsKey(CRMUSERNAME.ToLower()) == true)
                    return appConfigureTable[CRMUSERNAME.ToLower()];
                else
                    return string.Empty;
            }
        }

        /// <summary>
        /// Hardcoded property for CrmUserPassword
        /// </summary>
        internal string CrmUserPassword {
            get {
                if (appConfigureTable.ContainsKey(CRMUSERPASSWORD.ToLower()) == true)
                    return appConfigureTable[CRMUSERPASSWORD.ToLower()];
                else
                    return string.Empty;
            }
        }

        #endregion

        #region Hardcoded Email

        /// <summary>
        /// Hardcoded property for EmailUrl/AZUREEMAILURL
        /// </summary>
        internal string EmailUrl {
            get {
                if (appConfigureTable.ContainsKey(AZUREEMAILURL.ToLower()) == true)
                    return appConfigureTable[AZUREEMAILURL.ToLower()];
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// Hardcoded property for EmailTenantid/AZUREEMAILTENANTID
        /// </summary>
        internal string EmailTenantid {
            get {
                if (appConfigureTable.ContainsKey(AZUREEMAILTENANTID.ToLower()) == true)
                    return appConfigureTable[AZUREEMAILTENANTID.ToLower()];
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// Hardcoded property for EmailClientId/AZUREEMAILCLIENTID
        /// </summary>
        internal string EmailClientId {
            get {
                if (appConfigureTable.ContainsKey(AZUREEMAILCLIENTID.ToLower()) == true)
                    return appConfigureTable[AZUREEMAILCLIENTID.ToLower()];
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// Hardcoded property for EmailClientSecret/AZUREEMAILCLIENTSECRET
        /// </summary>
        internal string EmailClientSecret {
            get {
                if (appConfigureTable.ContainsKey(AZUREEMAILCLIENTSECRET.ToLower()) == true)
                    return appConfigureTable[AZUREEMAILCLIENTSECRET.ToLower()];
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// Hardcoded property for EmailClientSecret/AZUREEMAILAFSENDEREMAIL
        /// </summary>
        internal string EmailAfsenderMailAdressse {
            get {
                if (appConfigureTable.ContainsKey(AZUREEMAILAFSENDEREMAIL.ToLower()) == true)
                    return appConfigureTable[AZUREEMAILAFSENDEREMAIL.ToLower()];
                else
                    return string.Empty;
            }
        }
        /// <summary>
        /// Hardcoded property for modtagereEmail
        /// </summary>
        internal string[] EmailModtagere {
            get {
                if (appConfigureTable.ContainsKey(MODTAGEREEMAIL.ToLower()) == true)
                    return appConfigureTable[MODTAGEREEMAIL.ToLower()].Split(',', ';');
                else
                    return null;
            }
        }

        #endregion

        #region Hardcoded MSMQ

        /// <summary>
        /// Hardcoded property for gimsmqServer
        /// </summary>
        internal string GIMsmqServer {
            get {
                if (appConfigureTable.ContainsKey(GIMSMQSERVER.ToLower()) == true)
                    return appConfigureTable[GIMSMQSERVER.ToLower()];
                else
                    return string.Empty;
            }
        }
        #endregion

        #region validering

        /// <summary>
        /// Valider parametre
        /// - Check om der var problemer med at indlæse args
        /// - Check om alle krævede parametre var angivet (ikke om indhold er ok)
        /// </summary>
        /// <returns>True hvis ok</returns>

        internal bool ValidateRequired()
        {
            if (this.ContainsKey("?") == true || this.ContainsKey("Invalid") == true)
            {
                string msg = $"Der skete en fejl i forsøget på at parse parametre, angiv disse parametre:{string.Join(",", this.requiredParametre)}";
                TraceMessage(msg);
                return false;
            }

            if (this.ContainsAll(requiredParametre) == false)
            {
                string msg = $"Der mangler parametre, angiv alle disse parametre:{string.Join(",", this.requiredParametre)}";
                TraceMessage(msg);
                return false;
            }

            return true;
        }


        #region Valider Parametre for CRM
        /// <summary>
        /// Valider om nødvendige parametre til CRM er angivet
        /// </summary>
        /// <returns></returns>
        internal bool ValidateCrmConfiguration()
        {
            bool altOk = true;
            if (appConfigureTable.ContainsKey(CRMCONNECTIONSTEMPLATE.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: CrmConnectionTemplate var ikke udfyldt ");
                altOk = false;
            }
            if (appConfigureTable.ContainsKey(CRMSERVERNAME.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: CrmServerName var ikke udfyldt ");
                altOk = false;
            }
            if (appConfigureTable.ContainsKey(CRMORGANISATIONNAME.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: CrmOrganisationName var ikke udfyldt ");
                altOk = false;
            }
            if (appConfigureTable.ContainsKey(CRMUSERNAME.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: CrmUserName var ikke udfyldt ");
                altOk = false;
            }
            if (appConfigureTable.ContainsKey(CRMUSERPASSWORD.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: CrmUserPassword var ikke udfyldt ");
                altOk = false;
            }
            return altOk;
        }
        #endregion

        #region Valider Parametre for MSMQ
        /// <summary>
        /// Valider om nødvendige parametre for et job fra MSMQ var angivet
        /// </summary>
        /// <returns></returns>
        internal bool ValidateMSMQConfiguration()
        {
            bool altOk = true;
            if (appConfigureTable.ContainsKey(GIMSMQSERVER.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: gimsmqServer var ikke udfyldt ");
                altOk = false;
            }
            if (appConfigureTable.ContainsKey(MSGID.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: msgID var ikke udfyldt ");
                altOk = false;
            }
            return altOk;
        }
        #endregion

        #region Valider Parametre for E-Mail
        /// <summary>
        /// Valider om nødvendige parametre for send af email var til stede
        /// - Denne validerer ikke om der er email modtagere da det er et optional parameter
        /// </summary>
        /// <returns></returns>
        internal bool ValidateEmailConfiguration()
        {
            bool altOk = true;
            if (appConfigureTable.ContainsKey(AZUREEMAILURL.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: Azure.Email.Url var ikke udfyldt ");
                altOk = false;
            }
            if (appConfigureTable.ContainsKey(AZUREEMAILTENANTID.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: Azure.Email.TenantID var ikke udfyldt ");
                altOk = false;
            }
            if (appConfigureTable.ContainsKey(AZUREEMAILCLIENTID.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: Azure.Email.ClientID var ikke udfyldt ");
                altOk = false;
            }
            if (appConfigureTable.ContainsKey(AZUREEMAILCLIENTSECRET.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: Azure.Email.ClientSecret var ikke udfyldt ");
                altOk = false;
            }
            if (appConfigureTable.ContainsKey(AZUREEMAILAFSENDEREMAIL.ToLower()) == false)
            {
                TraceMessage($"Validerign AppSetting/Parametre: Azure.Email.AfsenderEmail var ikke udfyldt ");
                altOk = false;
            }
            return altOk;
        }

        /// <summary>
        /// Valider om nødvendige parametre for afsendelse af email var til stede
        /// - denne validerer om der også er en email modtager, hvis denne retunerer false kan afsendelse af mail undlades
        /// </summary>
        /// <returns></returns>
        internal bool ValidateEmailConfigurationOgEmailModtager()
        {
            bool altOk = this.ValidateEmailConfiguration();
            if (altOk == true)
            {
                if (appConfigureTable.ContainsKey(MODTAGEREEMAIL.ToLower()) == false)
                {
                    TraceMessage($"Validerign AppSetting/Parametre: modtagereEmail var ikke udfyldt ");
                    altOk = false;
                }
            }
            return altOk;
        }

        #endregion

        /// <summary>
        /// Send en besked til console og Serilog
        /// </summary>
        /// <param name="msg"></param>
        private void TraceMessage(string msg)
        {
            Console.WriteLine(msg);
            Serilog.Log.Error(msg);
        }
        #endregion

        #region Private metoder
        /// <summary>
        /// Indlæs parametre fra Environment variable
        /// </summary>
        private void ConfigureFromAppEnvironment()
        {
            string logPath = this.GetEnvironmentVariable(LOGPATH.ToLower());
            if (string.IsNullOrEmpty(logPath) == false)
                AddConfiguration(LOGPATH.ToLower(), Specialtegn.DecodeFromHtmlEntityName(logPath));

            string CrmConnectionTemplate = this.GetEnvironmentVariable(CRMCONNECTIONSTEMPLATE.ToLower());
            if (string.IsNullOrEmpty(CrmConnectionTemplate) == false)
                AddConfiguration(CRMCONNECTIONSTEMPLATE.ToLower(), Specialtegn.DecodeFromHtmlEntityName(CrmConnectionTemplate));
            else
            {
                // i en periode vil vi indlæse string fra CrmConnectionString til variabel CrmConnectionTemplate, det giver os nogen tid til at ændre variabel navn diverse steder
                string CrmConnectionString = this.GetEnvironmentVariable(CRMCONNECTIONSTEMPLATE.ToLower());
                if (string.IsNullOrEmpty(CrmConnectionString) == false)
                    AddConfiguration(CRMCONNECTIONSTEMPLATE.ToLower(), Specialtegn.DecodeFromHtmlEntityName(CrmConnectionString));
            }

            string CrmServerName = this.GetEnvironmentVariable(CRMSERVERNAME.ToLower());
            if (string.IsNullOrEmpty(CrmServerName) == false)
                AddConfiguration(CRMSERVERNAME.ToLower(), Specialtegn.DecodeFromHtmlEntityName(CrmServerName));

            string CrmOrganisationName = this.GetEnvironmentVariable(CRMORGANISATIONNAME.ToLower());
            if (string.IsNullOrEmpty(CrmOrganisationName) == false)
                AddConfiguration(CRMORGANISATIONNAME.ToLower(), Specialtegn.DecodeFromHtmlEntityName(CrmOrganisationName));

            string CrmUserName = this.GetEnvironmentVariable(CRMUSERNAME.ToLower());
            if (string.IsNullOrEmpty(CrmUserName) == false)
                AddConfiguration(CRMUSERNAME.ToLower(), Specialtegn.DecodeFromHtmlEntityName(CrmUserName));

            string CrmUserPassword = this.GetEnvironmentVariable(CRMUSERPASSWORD.ToLower());
            if (string.IsNullOrEmpty(CrmUserPassword) == false)
                AddConfiguration(CRMUSERPASSWORD.ToLower(), CrmUserPassword);

            string TimeOutMinutter = this.GetEnvironmentVariable(TIMEOUTMINUTTER.ToLower());
            if (string.IsNullOrEmpty(TimeOutMinutter) == false)
                AddConfiguration(TIMEOUTMINUTTER.ToLower(), TimeOutMinutter);

            string reuseserviceclient = this.GetEnvironmentVariable(REUSESERVICECLIENT.ToLower());
            if (string.IsNullOrEmpty(reuseserviceclient) == false)
                AddConfiguration(REUSESERVICECLIENT.ToLower(), reuseserviceclient);

            string secondstosleep = this.GetEnvironmentVariable(SECONDSTOSLEEP.ToLower());
            if (string.IsNullOrEmpty(secondstosleep) == false)
                AddConfiguration(SECONDSTOSLEEP.ToLower(), secondstosleep);
            
            string maxwaitcount = this.GetEnvironmentVariable(MAXWAITCOUNT.ToLower());
            if (string.IsNullOrEmpty(maxwaitcount) == false)
                AddConfiguration(MAXWAITCOUNT.ToLower(), maxwaitcount);

            foreach (string key in ekstraParametre)
            {
                string value = this.GetEnvironmentVariable(key);
                if (string.IsNullOrEmpty(value) == false)
                    AddConfiguration(key.ToLower(), Specialtegn.DecodeFromHtmlEntityName(value));
            }
        }

        private string GetEnvironmentVariable(string name)
        {
            try
            {
                return Environment.GetEnvironmentVariable(name);
            }
            catch
            { }
            return string.Empty;
        }

        /// <summary>
        /// Indlæs parametre fra applikationens .config fil
        /// </summary>
        private void ConfigureFromAppSettings()
        {
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(LOGPATH) == true)
                AddConfiguration(LOGPATH.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[LOGPATH]));

            #region App Settings vedrørende CRM
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(CRMCONNECTIONSTEMPLATE) == true)
                AddConfiguration(CRMCONNECTIONSTEMPLATE.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[CRMCONNECTIONSTEMPLATE]));
            else
            {
                // i en periode vil vi indlæse string fra CrmConnectionString til variabel CrmConnectionTemplate, det giver os nogen tid til at ændre variabel navn diverse steder
                if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(CRMCONNECTIONSTRING) == true)
                    AddConfiguration(CRMCONNECTIONSTEMPLATE.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[CRMCONNECTIONSTRING]));
            }

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(CRMSERVERNAME) == true)
                AddConfiguration(CRMSERVERNAME.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[CRMSERVERNAME]));

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(CRMORGANISATIONNAME) == true)
                AddConfiguration(CRMORGANISATIONNAME.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[CRMORGANISATIONNAME]));

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(CRMUSERNAME) == true)
                AddConfiguration(CRMUSERNAME.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[CRMUSERNAME]));

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(CRMUSERPASSWORD) == true)
                AddConfiguration(CRMUSERPASSWORD.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[CRMUSERPASSWORD]));

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(TIMEOUTMINUTTER) == true)
                AddConfiguration(TIMEOUTMINUTTER.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[TIMEOUTMINUTTER]));

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(REUSESERVICECLIENT) == true)
                AddConfiguration(REUSESERVICECLIENT.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[REUSESERVICECLIENT]));
            #endregion

            #region App Settings vedrørende Email config
            //// Konfiguration af email ved fejl eller andet
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(AZUREEMAILURL) == true)
                AddConfiguration(AZUREEMAILURL.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[AZUREEMAILURL]));

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(AZUREEMAILTENANTID) == true)
                AddConfiguration(AZUREEMAILTENANTID.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[AZUREEMAILTENANTID]));

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(AZUREEMAILCLIENTID) == true)
                AddConfiguration(AZUREEMAILCLIENTID.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[AZUREEMAILCLIENTID]));

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(AZUREEMAILCLIENTSECRET) == true)
                AddConfiguration(AZUREEMAILCLIENTSECRET.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[AZUREEMAILCLIENTSECRET]));

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(AZUREEMAILAFSENDEREMAIL) == true)
                AddConfiguration(AZUREEMAILAFSENDEREMAIL.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[AZUREEMAILAFSENDEREMAIL]));

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(MODTAGEREEMAIL) == true)
                AddConfiguration(MODTAGEREEMAIL.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[MODTAGEREEMAIL]));

            #endregion

            #region App Settings vedrørende MSMQ
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(GIMSMQSERVER) == true)
                AddConfiguration(GIMSMQSERVER.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[GIMSMQSERVER]));
            // msgID indlæses IKKE fra AppSettings, men kun fra app args
            #endregion

            #region Mutex
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(SECONDSTOSLEEP) == true)
                AddConfiguration(SECONDSTOSLEEP.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[SECONDSTOSLEEP]));

            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(MAXWAITCOUNT) == true)
                AddConfiguration(MAXWAITCOUNT.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[MAXWAITCOUNT]));
            #endregion

            #region MSMQ
            if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(GIMSMQSERVER) == true)
                AddConfiguration(GIMSMQSERVER.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[GIMSMQSERVER]));

            #endregion

            // Forsøg at indlæse disse
            foreach (string key in ekstraParametre)
            {
                if (System.Configuration.ConfigurationManager.AppSettings.AllKeys.Contains(key) == true)
                    AddConfiguration(key.ToLower(), Specialtegn.DecodeFromHtmlEntityName(System.Configuration.ConfigurationManager.AppSettings[key]));
            }

        }

        /// <summary>
        /// Indlæs de angivne parametre fra argumenterne til programmet
        /// </summary>
        /// <param name="args"></param>
        private void ConfigureFromAppArgs(string[] args)
        {
            try
            {
                string[] keyvalue;
                string value;

                if ((null == args) || args.Length == 0)
                {
                    return;
                }

                foreach (string s in args)
                {

                    if ((s.StartsWith("-") || s.StartsWith("/")) && (s.Contains("=") && !s.Contains("?")))
                    {
                        // Strip off - or /
                        string key = s.Substring(1, s.Length - 1);
                        keyvalue = key.Split(new char[] { '=' }, 2); //Den skal kun deles i 2 selvom der måtte være flere =
                        key = keyvalue[0].ToString();
                        value = Specialtegn.DecodeFromHtmlEntityName(keyvalue[1].ToString());

                        if (value == null || value.Trim() == "")
                        {
                            AddConfiguration("Invalid", "true");
                            return;
                        }

                        AddConfiguration(key, value);
                    }
                    else
                    {
                        // otherwise this is invalid value
                        if (s.Contains("?"))
                        {
                            AddConfiguration("?", "true");
                        }
                        else
                        {
                            AddConfiguration("Invalid", "true");
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException outOfRangeEx)
            {
                Console.WriteLine("Arguments out of range, please check error log" + outOfRangeEx.Message + Environment.NewLine + outOfRangeEx.StackTrace);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + Environment.NewLine + ex.StackTrace);
            }
        }
        #endregion
    }
}