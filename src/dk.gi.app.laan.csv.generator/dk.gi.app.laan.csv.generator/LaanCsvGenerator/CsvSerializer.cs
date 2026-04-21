using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace dk.gi.crm.app.LaanCsvGenerator
{
	class CsvSerializer<T> where T : class
	{
		private List<Tuple<PropertyInfo, CsvMemberAttribute>> _typeMembers = null;

		#region .ctor
		/// <summary>
		/// Creates a new empty <see cref="CsvSerializer"/>
		/// </summary>
		public CsvSerializer ()
		{
			Initialize();
		}
		#endregion

		#region properties
		/// <summary>
		/// <para>The char used to separate individual columns.</para>
		/// <para>Default value is semi-colon (;) and defined in app.config.</para>
		/// </summary>
		public char ColumnSeparator { get; set; }

		/// <summary>
		/// <para>The culture used to format data.</para>
		/// <para>Default is danish (da-DK) and defined by a value in app.config</para>
		/// </summary>
		public CultureInfo FormattingCulture { get; set; }

		/// <summary>
		/// Whether or not to ignore null objects when generating data rows.
		/// </summary>
		public bool IgnoreNullObjects { get; set; }

		/// <summary>
		/// <para>The char used as a text qualifier when data contains 1 or more <see cref="ColumnSeparator"/> chars.</para>
		/// <para>Default value is quote (") and defined in app.config.</para>
		/// </summary>
		public char TextQualifier { get; set; }
		#endregion

		private void Initialize ()
		{
			// 2014-09-16 RMP: Set the default separator
			this.ColumnSeparator = Properties.Settings.Default.CSV_Separator;

			// 2014-09-16 RMP: Set the default text qualifier
			this.TextQualifier = Properties.Settings.Default.CSV_TextQualifier;

			// 2014-09-16 RMP: Set the default formatting culture
			this.FormattingCulture = Properties.Settings.Default.CSV_Formatting_Culture;

			// 2014-09-12 RMP: Initialize the collection storing the type members
			_typeMembers = new List<Tuple<PropertyInfo, CsvMemberAttribute>>();

			// 2014-09-12 RMP: Initialize a collection for type members with no Order property
			var membersWithNoOrder = new List<Tuple<PropertyInfo, CsvMemberAttribute>>();

			var props = typeof(T).GetProperties();
			var sortOrderOffset = props.Length;

			foreach (var p in props) {
				var cm = p.GetCustomAttributes(typeof(CsvMemberAttribute), true).FirstOrDefault() as CsvMemberAttribute;

				if (cm != null) {
					// 2014-09-11 RMP: Set the name of the property when not given in the attribute
					if (cm.Name == null)
						cm.Name = p.Name;

					// 2014-09-12 RMP: Add to appropriate collection based on the CsvMemberAttribute
					//                 having an order ord not
					if (cm.HasOrderValue()) {
						// 2014-09-11 RMP: Add to collection
						_typeMembers.Add(new Tuple<PropertyInfo, CsvMemberAttribute>(p, cm));
					}
					else
						membersWithNoOrder.Add(new Tuple<PropertyInfo, CsvMemberAttribute>(p, cm));
				}
			}

			// 2014-09-11 RMP: Sort the type members with orders on them
			_typeMembers = _typeMembers
				.OrderBy(x => x.Item2.Order)
				.ToList();

			// 2014-09-12 RMP: Add type members with no orders
			_typeMembers.AddRange(membersWithNoOrder);
		}

		/// <summary>
		/// Generates CSV content for the given objects including a header row.
		/// When generating the CSV content the default values for <see cref="ColumnSeparator"/>, <see cref="TextQualifier"/>,
		/// <see cref="FormattingCulture"/> and <see cref="IgnoreNullObjects"/> will be utilized.
		/// </summary>
		/// <param name="objects">The data to be serialized to CSV content.</param>
		/// <returns>A CSV string.</returns>
		public string Generate (IEnumerable<T> objects)
		{
			return this.Generate(
				objects
				, separator: this.ColumnSeparator
				, textQualifier: this.TextQualifier
				, formattingCulture: this.FormattingCulture
				, ignoreNullObjects: this.IgnoreNullObjects
				, includeHeader: true
			);
		}

		/// <summary>
		/// <para>Generates CSV content for a given set of <paramref name="objects"/>.</para>
		/// </summary>
		/// <param name="objects">The data to be serialized to CSV content.</param>
		/// <param name="separator">The separator to be used to when separating the data columns</param>
		/// <param name="textQualifier">The <see cref="char"/> used as a text qualifier when a data column contains either the <paramref name="separator"/> -or- the textQualifier.</param>
		/// <param name="formattingCulture"><see cref="CultureInfo"/> used when formatting the various values.</param>
		/// <param name="ignoreNullObjects">Whether or not to ignore a null object.</param>
		/// <param name="includeHeader">Whether or not to include a header in the generate result.</param>
		/// <returns>A CSV string.</returns>
		public string Generate (IEnumerable<T> objects, char separator, char textQualifier, CultureInfo formattingCulture, bool ignoreNullObjects, bool includeHeader)
		{
			// 2014-09-16 RMP: Define StringBuilder used when generating the CSV content
			var result = new StringBuilder();

			// 2014-09-16 RMP: Add header when requested
			if (includeHeader)
				result.AppendLine(this.GetHeader(separator, textQualifier));

			// 2014-09-16 RMP: Iterate over all the data objects and add header information
			foreach (var item in objects) {
				// 2014-09-16 RMP: If item is null and we should ignore null objects
				//                 move on to the next item...
				if (item == null && ignoreNullObjects)
					continue;

				// 2014-09-16 RMP: Add a line for the current item
				result.AppendLine(this.GetLine(item, separator, textQualifier, formattingCulture, ignoreNullObjects));
			}

			// 2014-09-16 RMP: yield the result
			return result.ToString();
		}

		/// <summary>
		/// Generates a header row using the default <see cref="ColumnSeparator"/> and <see cref="TextQualifier"/>
		/// </summary>
		/// <returns>A <see cref="string"/></returns>
		public string GetHeader ()
		{
			return this.GetHeader(
				separator: this.ColumnSeparator
				, textQualifier: this.TextQualifier
			);
		}
		/// <summary>
		/// Generates a header row using a given <paramref name="separator"/> and <paramref name="textQualifier"/>
		/// </summary>
		/// <param name="separator">The separator to be used to when separating the header columns</param>
		/// <param name="textQualifier">The <see cref="char"/> used as a text qualifier when a data column contains either the <paramref name="separator"/> -or- the textQualifier.</param>
		/// <returns>A <see cref="string"/></returns>
		public string GetHeader (char separator, char textQualifier)
		{
			return String.Join(separator.ToString(), _typeMembers.Select(x => {
				var header = x.Item2.Name;
				var addTextQualifiers = (header.IndexOf(separator) != -1 ? 1 : 0) + (header.IndexOf(textQualifier) != -1 ? 2 : 0);

				if (addTextQualifiers > 0) {
					// 2014-09-16 RMP: When addTextQualifiers is (> 1) greater than 1; the value includes text-qualifiers which needs to be doubled
					if (addTextQualifiers > 1)
						header = header.Replace(new string(textQualifier, 1), new string(textQualifier, 2));

					// 2014-09-16 RMP: Add qualifiers to the header
					header = textQualifier.ToString() + header + textQualifier.ToString();
				}

				return header;
			}).ToArray());
		}

		/// <summary>
		/// Formats a single object into a CSV data line using the default <see cref="ColumnSeparator"/>, <see cref="TextQualifier"/>, 
		/// <see cref="FormattingCulture"/> and <see cref="IgnoreNullObjects"/> values.
		/// </summary>
		/// <param name="obj">The object which should be converted into a CSV data line.</param>
		/// <returns>A <see cref="string"/>.</returns>
		public string GetLine (T obj)
		{
			return this.GetLine(
				obj
				, separator: this.ColumnSeparator
				, textQualifier: this.TextQualifier
				, formattingCulture: this.FormattingCulture
				, ignoreNullObject: this.IgnoreNullObjects
			);
		}
		/// <summary>
		/// Formats a single object into a CSV data line using a given <paramref name="separator"/>
		/// </summary>
		/// <param name="obj">The object which should be converted into a CSV data line.</param>
		/// <param name="separator">The separator to be used to when separating the data columns</param>
		/// <param name="textQualifier">The <see cref="char"/> used as a text qualifier when a data column contains either the <paramref name="separator"/> -or- the textQualifier.</param>
		/// <param name="formattingCulture"><see cref="CultureInfo"/> used when formatting the various values.</param>
		/// <param name="ignoreNullObject">Whether or not to ignore a null object.</param>
		/// <returns>A <see cref="string"/>.</returns>
		public string GetLine (T obj, char separator, char textQualifier, CultureInfo formattingCulture, bool ignoreNullObject)
		{
			// 2014-09-16 RMP: When instructed to ignore null objects and object is null
			//                 yield an empty string
			if (ignoreNullObject && obj == null)
				return String.Empty;

			var result = new StringBuilder();

			// 2014-09-16 RMP: Iterate over all _typeMembers
			//                 This will generate the desired number of columns
			for (int index = 0; index < _typeMembers.Count; index++) {
				// 2014-09-16 RMP: Make sure we have an object to retrieve a value from
				if (obj != null) {
					// 2014-09-16 RMP: Read the current typeMember
					var member = _typeMembers[index];

					// 2014-09-16 RMP: Read the value of the current typeMember
					var typeMemberValue = member.Item1.GetValue(obj, null);

					// 2014-09-16 RMP: Define variable for textual content of current typeMember
					string typeMemberTextValue = "";

					// 2014-09-16 RMP: Format the typeMember value
					//                 1st test whether or not we have a typeMemberValue
					if (typeMemberValue != null)
						// 2014-09-16 RMP: We have a typeMemberValue format this value
						typeMemberTextValue = FormatObject(typeMemberValue, member.Item2.OutputFormat, formattingCulture);
					else
						// 2014-09-16 RMP: We don't have a typeMemberValue format an empty string
						typeMemberTextValue = FormatObject("", member.Item2.OutputFormat, formattingCulture);

					// 2014-09-16 RMP: do we have a maximumlength for the current typeMember?
					if (member.Item2.HasMaxLength()) {
						// 2014-09-16 RMP: Yes, we have a defined MaxLength, make sure we don't exceed the defined
						//                 maximum length
						if (typeMemberTextValue.Length > member.Item2.MaxLength) {
							typeMemberTextValue = typeMemberTextValue.Substring(0, member.Item2.MaxLength);
						}
					}

					// 2014-09-16 RMP: Do we have a text qualifier in the current typeMemberTextValue?
					//                 If we do they needs to be doubled
					var addTextQualifiers = (typeMemberTextValue.IndexOf(separator) != -1? 1 : 0) + (typeMemberTextValue.IndexOf(textQualifier) != -1? 2: 0);

					// 2014-09-16 RMP: Can we add the current value without also adding text qualifiers?
					//                 we can when addTextQualifiers equals zero (0)
					if (addTextQualifiers == 0)
						result.Append(typeMemberTextValue);
					else {
						// 2014-09-16 RMP: When addTextQualifiers is (> 1) greater than 1; the value includes text-qualifiers which needs to be doubled
						if (addTextQualifiers > 1)
							typeMemberTextValue = typeMemberTextValue.Replace(new string(textQualifier, 1), new string(textQualifier, 2));

						// 2014-09-16 RMP: Add text qualifiers and the current value
						result.Append(textQualifier);
						result.Append(typeMemberTextValue);
						result.Append(textQualifier);
					}
				}

				// 2014-09-16 RMP: If this is not the last column add a separator
				if (index < _typeMembers.Count - 1)
					result.Append(separator);
			}

			return result.ToString();
		}

		/// <summary>
		/// Formats a given <paramref name="value"/> using a <paramref name="formattingCulture"/> and <see cref="formatProvider"/>
		/// </summary>
		/// <param name="value">The value to be formatted, this should not be null.</param>
		/// <param name="formatProvider">A composite format string, if this is null <c>{0}</c> will be used.</param>
		/// <param name="formattingCulture">The culture to be used when formatting.</param>
		/// <returns>A formatted string.</returns>
		private static string FormatObject (object value, string formatProvider, CultureInfo formattingCulture)
		{
			return String.Format(
				formattingCulture
				, (String.IsNullOrWhiteSpace(formatProvider) ? "{0}" : formatProvider)
				, value
			);
		}
	}
}
