/// <summary>
///
/// Version: 2022 12 19
/// Sidste ændring: Changed Result pattern to be AppStatus and not AppStatus.StateCode
///
/// 2022 03 10 Denne fil er tilføjet til standard template
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi
{
    /// <summary>
    /// En status til at styre om der opstår fejl, og i så fald hvilken type fejl
    ///   0 alt er ok
    ///   - Alle negative tal er fejl i forbindelse med opstart af program og den generelle kode
    ///   + Alle positive tal kan bruges efter eget ønske i det enkelte program
    /// </summary>
    public class AppStatus
    {
        /// <summary>
        /// Enum der indeholder de forskellige værdier
        /// Hvis du tilføjer værdier i denne enum, så skal du også tilføje dem i SetStatus property
        /// </summary>
        public enum StateCode : int
        {
            AppUventetFejlIProgramKode = -9,
            AppIngenCRMForbindelse = -8,
            AppExceptionInCode = -7,
            AppRequiredEkstraParamMissing = -6,
            AppRequiredMsmqParmsMissing = -5,
            AppRequiredEmailParmsMissing = -4,
            AppRequiredCrmParmsMissing = -3,
            AppRequiredParmsMissing = -2,
            AppIsRunning = -1,
            OK = 0,
        }

        /// <summary>
        /// Sæt status kode og samtidig en standard fejl tekst
        /// </summary>
        public StateCode SetStatus
        {
            set
            {
                this._statecode = value;
                // Herunder sættes den interne variable _statemsg fordi der er en property som også sætte statecode, og det vil vi ikke have
                switch ((int)this._statecode)
                {
                    case (int)StateCode.AppUventetFejlIProgramKode:
                        _statemsg = "Der opstod en fejl undervejs i koden";
                        break;
                    case (int)StateCode.AppIngenCRMForbindelse:
                        _statemsg = "Der kunne ikke forbindes til CRM";
                        break;
                    case (int)StateCode.AppExceptionInCode:
                        _statemsg = "Der opstod en exception";
                        break;
                    case (int)StateCode.AppRequiredEkstraParamMissing:
                        _statemsg = "Et (eller flere)af de krævede parametre mangler(ekstraParametre)";
                        break;
                    case (int)StateCode.AppRequiredMsmqParmsMissing:
                        _statemsg = "Et (eller flere)af de krævede MSMQ parametre mangler";
                        break;
                    case (int)StateCode.AppRequiredEmailParmsMissing:
                        _statemsg = "Et (eller flere)af de krævede E-mail parametre mangler";
                        break;
                    case (int)StateCode.AppRequiredCrmParmsMissing:
                        _statemsg = "Et (eller flere)af de krævede CRM parametre mangler";
                        break;
                    case (int)StateCode.AppRequiredParmsMissing:
                        _statemsg = "Et (eller flere)af de krævede parametre mangler";
                        break;
                    case (int)StateCode.AppIsRunning:
                        _statemsg = "App kører i forvejen";
                        break;
                    case (int)StateCode.OK:
                        _statemsg = "";
                        break;
                    default:
                        _statemsg = "Ukendt statecode 2 statemsg";
                        break;
                }
            }
        }
        public StateCode statecode
        {
            get { return _statecode; }
        }

        /// <summary>
        /// Sæt status til manglende parameter, her kan samtidigt sættes navnet på parameter
        /// </summary>
        public string SetStatusManglendeParameter
        {
            set
            {
                this._statecode = StateCode.AppRequiredEkstraParamMissing;
                _statemsg = $"Applikation mangler {value} parameter";
            }
        }

        /// <summary>
        /// Intern variabel til status
        /// Default everything is OK
        /// </summary>
        private StateCode _statecode = StateCode.OK;

        /// <summary>
        /// String variabel til status tekst
        /// Når den sættes vil statecode automatisk blive sat til AppUventetFejlIProgramKode
        /// </summary>
        public string SetStatusTekstmsg
        {
            set
            {
                // Hvis det bare er en tom string, så er det formentligt ikke seriøst ment
                if (string.IsNullOrEmpty(value) == false)
                    this._statecode = StateCode.AppUventetFejlIProgramKode;
                this._statemsg = value;
            }
        }

        /// <summary>
        /// String variabel til status tekst
        /// Når den sættes vil statecode IKKE blive rørt og kan bruges til at sætte en "Alt gik gode besked"
        /// </summary>
        public string SetStatusTekstOKmsg
        {
            set
            {
                this._statemsg = value;
            }
        }

        public string GetStatusTekstmsg
        {
            get
            {
                return _statemsg;
            }
        }
        internal string _statemsg = string.Empty;
    }
}