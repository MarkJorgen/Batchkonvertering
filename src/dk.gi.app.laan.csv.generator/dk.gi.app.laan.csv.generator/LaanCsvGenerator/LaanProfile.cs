using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dk.gi.crm.app.LaanCsvGenerator
{
	class LaanProfile
	{
		public LaanProfile ()
		{
			this.KreditInstitutCVR = Properties.Settings.Default.GI_CVR;
			this.OpgørelsesDato = new DateTime(DateTime.Now.Year - 1, 12, 31);
		}

		[CsvMember(Name = "INSTCVR", Order = 1, OutputFormat = "{0,8:00000000}")]
		public string KreditInstitutCVR { get; private set; }

		[CsvMember(Name = "AFTNR", Order = 2, MaxLength = 7)]
		public string AftaleNummer { get; set; }

		[CsvMember(Name = "OPGDATO", Order = 3, OutputFormat = "{0:yyyyMMdd}")]
		public DateTime OpgørelsesDato { get; private set; }

		[CsvMember(Name = "AAR", Order = 4, OutputFormat = "{0:0000}")]
		public int År { get; set; }

		[CsvMember(Name = "YDELSE", Order = 5)]
		public decimal ÅrligYdelse { get; set; }

		[CsvMember(Name = "YDELSES", Order = 6)]
		public decimal ÅrligYdelseHvisRentenStiger1Pct { get { return this.ÅrligYdelse; } }

		[CsvMember(Name = "YDELSEF", Order = 7)]
		public decimal ÅrligYdelseHvisRentenFalder1Pct { get { return this.ÅrligYdelse; } }

		[CsvMember(Name = "RESTGALDN", Order = 8)]
		public decimal NominelRestgæld { get; set; }
	}
}
