using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace dk.gi.crm.app.LaanCsvGenerator
{
	[DataContract(Namespace = "")]
	class Laan
	{
		public Laan ()
		{
			// 2014-09-11 RMP: Sæt properties der er fixet til en bestemt værdi
			this.KreditInstitutCVR = Properties.Settings.Default.GI_CVR;
			this.AftaleType = Properties.Settings.Default.GI_Aftale_Type;
			this.AftaleBeskrivelse = Properties.Settings.Default.GI_Aftale_Beskrivelse;
			this.RisikoKlassificering = Properties.Settings.Default.GI_RisikoKlassificering;
			this.LængdePåRentetilpasningsPeriode = " ";
			this.NaesteRentetilpasning = "";
			this.ABF_NR = "";
			this.OpgørelsesDato = new DateTime(DateTime.Now.Year - 1, 12, 31);
		}

		/// <summary>
		/// Dette skal indeholde GI's CVR nummer.
		/// Værdien hentes fra app.config og kan ikke sættes.
		/// </summary>
		[CsvMember(Name = "INSTCVR", Order = 1, OutputFormat = "{0,8:00000000}")]
		public string KreditInstitutCVR { get; private set; }

		/// <summary>
		/// <para>Dette skal være 31/12 året før indberetningstidspunktet.</para>
		/// </summary>
		[CsvMember(Name = "OPGDATO", Order = 2, OutputFormat = "{0:yyyyMMdd}")]
		public DateTime OpgørelsesDato { get; private set; }

		[CsvMember(Name = "CVRNR", Order = 3, OutputFormat = "{0,8:00000000}")]
		public string CVRNR { get; set; }

		/// <summary>
		/// 2014-09-11 RMP: Denne benyttes ikke pt. og er fixed til <see cref="String.Empty"/> jf. aftale med Henning Larsen
		/// </summary>
		[CsvMember(Name = "ABFNR", Order = 4)]
		public string ABF_NR { get; set; }

		[CsvMember(Name = "AFTNR", Order = 5, MaxLength = 20)]
		public string LåneNummer { get; set; }

		/// <summary>
		/// 2014-09-11 RMP: Denne benyttes ikke pt. og er fixed til 99 jf. aftale med Henning Larsen
		/// 
		/// Dette er en GI specifik kode, som vil blive oplyst senere (lige nu sættes den til 99).
		/// </summary>
		[CsvMember(Name = "AFTTYPE", Order = 6)]
		public string AftaleType { get; set; }

		/// <summary>
		/// 2014-09-11 RMP: Denne benyttes ikke pt. og er fixed til " " jf. aftale med Henning Larsen
		/// 
		/// Hvis der er tale om et rentetilpasningslån anføres her længden på rentetilpasningsperioden i antal år med to decimaler.
		/// Er der ikke tale om et rentetilpasningslån inberettes et blank tegn.
		/// </summary>
		[CsvMember(Name = "RENTEP", Order = 7)]
		public string LængdePåRentetilpasningsPeriode { get; set; }

		/// <summary>
		/// 2014-09-11 RMP: Denne hænger sammen med <see cref="AftaleType"/> og vil have en fixed jf. aftale med Henning Larsen
		///                 Værdien er angivet i app.config
		/// </summary>
		[CsvMember(Name = "AFTBESKRIV", Order = 8)]
		public string AftaleBeskrivelse { get; private set; }

		/// <summary>
		/// Restgælden på lånet
		/// 
		/// 2014-09-11 RMP: Jf. aftale med Henning Larsen så kopieres værdien af denne property til
		///                 <see cref="NominelRestgæld"/>, <see cref="RestgældHvisRentenStiger1Pct"/> og <see cref="RestgældHvisRentenFalder1Pct"/>
		/// </summary>
		[CsvMember(Name = "RESTGALD", Order = 9, OutputFormat = "{0:###########0}")]
		public decimal Restgæld {
			get { return _restgæld; }
			set
			{
				_restgæld = value;

				// 2014-09-11 RMP: Sæt properties der arver denne værdi
				this.NominelRestgæld = value;
				this.RestgældHvisRentenFalder1Pct = value;
				this.RestgældHvisRentenStiger1Pct = value;
			}
		} private decimal _restgæld = 0;

		/// <summary>
		/// Angiver om der er afdrag på lånet (1 = ja, 0 = nej)
		/// 
		/// 2014-12-04 RMP: Så tog det kun en dag for Henning at ændre mening, det er opgørelsesdatoen + 1 år der skal være større end datoen i
		///                 afdrag påbegyndes.
		/// 2014-12-03 RMP: Efter aftale med Henning Larsen, sættes denne til 1, når <see cref="OpgørelsesDato"/> er større end <see cref="AfdragPaabegyndes"/>
		///                 ellers sættes den til 0.
		/// </summary>
		[CsvMember(Name = "AFDRAG", Order = 10)]
		public int AfdragIIndbetalingsaaret {
			get
			{
				return this.OpgørelsesDato.AddYears(1) > this.AfdragPaabegyndes ? 1 : 0;
			}
		}

		[CsvMember(Name = "STARTAFDRAG", Order = 11, OutputFormat = "{0:yyyyMMdd}")]
		public DateTime AfdragPaabegyndes { get; set; }

		/// <summary>
		/// 2014-09-11 RMP: Denne benyttes ikke pt. og er fixed til <see cref="String.Empty"/> jf. aftale med Henning Larsen
		/// </summary>
		[CsvMember(Name = "RENTET", Order = 12)]
		public string NaesteRentetilpasning { get; private set; }

		/// <summary>
		/// Restløbetid i år med to decimaler og maksimalt 5 cifre.
		/// 
		/// 2014-12-03 RMP: Henning oplyse at decimaler helst skal være i kvarte, altså (0,00; 0,25; 0,50 og/eller 0,75).
		/// </summary>
		[CsvMember(Name = "RESTLOBE", Order = 13, OutputFormat = "{0:##0.00}")]
		public double? RestLoebetid { get; set; }

		/// <summary>
		/// 2014-09-11 RMP: Denne er fixed jf. aftale med Henning Larsen.
		///                 Værdien er angivet i app.config
		/// </summary>
		[CsvMember(Name = "RISIKO", Order = 14, OutputFormat = "{0:D}")]
		public RisikoKlassificeringOptions RisikoKlassificering { get; private set; }

		/// <summary>
		/// Nominel restgæld (ultimo året), værdien er numerisk i hele kroner.
		/// 
		/// 2014-09-11 RMP: Efter aftale med Henning Larsen, så er denne værdi den samme som <see cref="Restgæld"/> og værdien
		///                 blive sat når værdien af <see cref="Restgæld"/> sættes
		/// </summary>
		[CsvMember(Name = "RESTGALDN", Order = 15)]
		public decimal NominelRestgæld { get; private set; }

		/// <summary>
		/// Restgæld ved indfrielse, hvis renten stiger 1 procent point (ultimo året), værdien er numerisk i hele kroner.
		/// 
		/// 2014-09-11 RMP: Efter aftale med Henning Larsen, så er denne værdi den samme som <see cref="Restgæld"/> og værdien
		///                 blive sat når værdien af <see cref="Restgæld"/> sættes
		/// </summary>
		[CsvMember(Name = "RESTGALDS", Order = 16)]
		public decimal RestgældHvisRentenStiger1Pct { get; private set; }

		/// <summary>
		/// Restgæld ved indfrielse, hvis renten falder 1 procent point (ultimo året), værdien er numerisk i hele kroner.
		/// 
		/// 2014-09-11 RMP: Efter aftale med Henning Larsen, så er denne værdi den samme som <see cref="Restgæld"/> og værdien
		///                 blive sat når værdien af <see cref="Restgæld"/> sættes
		/// </summary>
		[CsvMember(Name = "RESTGALDF", Order = 17)]
		public decimal RestgældHvisRentenFalder1Pct { get; private set; }

		#region methods
		#endregion
	}
}
