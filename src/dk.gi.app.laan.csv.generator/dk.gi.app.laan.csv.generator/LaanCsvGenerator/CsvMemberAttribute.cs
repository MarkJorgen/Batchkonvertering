using System;

namespace dk.gi.crm.app.LaanCsvGenerator
{
	[AttributeUsage(AttributeTargets.Property)]
	class CsvMemberAttribute : Attribute
	{
		#region .ctor
		/// <summary>
		/// Opretter en ny <see cref="CsvMemberAttribute"/>
		/// </summary>
		public CsvMemberAttribute () {}
		/// <summary>
		/// Opretter en ny <see cref="CsvMemberAttribute"/> med et given kolonne navn (<paramref name="name"/>)
		/// </summary>
		/// <param name="name">Navnet på den aktuelle CSV-kolonne, bruges til headeren i CSV-filen.</param>
		public CsvMemberAttribute (string name)
		{
			this.Name = name;
		}
		#endregion

		/// <summary>
		/// Navn på CSV kolonnen
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Sorteringsrækkefølgen for CSV kolonnen.
		/// </summary>
		public int Order {
			get { return _order.GetValueOrDefault(); }
			set { _order = value; }
		} private int? _order = null;

		/// <summary>
		/// Den maksimale længde på data indhold i CSV kolonnen
		/// </summary>
		public int MaxLength
		{
			get { return _maxLength.GetValueOrDefault(); }
			set { _maxLength = value; }
		} private int? _maxLength = null;

		/// <summary>
		/// <para>Streng brugt til at formaterer data inden de skrives til en CSV-fil.</para>
		/// <para>For enums kan man vise tallet ved at angive "{0:D}", se mere her: http://msdn.microsoft.com/en-us/library/c3s1ez6e(v=vs.110).aspx </para>
		/// </summary>
		public string OutputFormat { get; set; }

		/// <summary>
		/// Angiver om en <see cref="Order"/> værdi er blevet sat
		/// </summary>
		/// <returns><c>true</c> når en <see cref="Order"/> er blevet sat ellers <c>false</c></returns>
		public bool HasOrderValue () { return _order.HasValue; }

		/// <summary>
		/// Angiver om en  <see cref="MaxLength"/> værdi er blevet sat
		/// </summary>
		/// <returns><c>true</c> når en <see cref="MaxLength"/>  er blevet sat ellers <c>false</c></returns>
		public bool HasMaxLength () { return _maxLength.HasValue; }
	}
}
