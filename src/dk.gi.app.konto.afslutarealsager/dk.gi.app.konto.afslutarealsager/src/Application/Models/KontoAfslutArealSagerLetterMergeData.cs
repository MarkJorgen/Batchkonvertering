using System;
using System.Globalization;

namespace dk.gi.app.konto.afslutarealsager.Application.Models
{
    public sealed class KontoAfslutArealSagerLetterMergeData
    {
        public string Sagsnr { get; }
        public string Kontonr { get; }
        public string Ejendom { get; }
        public string DagsDato { get; }
        public string FraDato { get; }
        public string TilDato { get; }
        public string Navn { get; }
        public string Adresse { get; }
        public string Postnr { get; }
        public string By { get; }

        private KontoAfslutArealSagerLetterMergeData(
            string sagsnr,
            string kontonr,
            string ejendom,
            string dagsDato,
            string fraDato,
            string tilDato,
            string navn,
            string adresse,
            string postnr,
            string by)
        {
            Sagsnr = sagsnr ?? string.Empty;
            Kontonr = kontonr ?? string.Empty;
            Ejendom = ejendom ?? string.Empty;
            DagsDato = dagsDato ?? string.Empty;
            FraDato = fraDato ?? string.Empty;
            TilDato = tilDato ?? string.Empty;
            Navn = navn ?? string.Empty;
            Adresse = adresse ?? string.Empty;
            Postnr = postnr ?? string.Empty;
            By = by ?? string.Empty;
        }

        public static KontoAfslutArealSagerLetterMergeData Create(KontoAfslutArealSagerCandidate candidate, DateTime? today = null)
        {
            DateTime now = today ?? DateTime.Today;
            DateTime fromDate = candidate.LastAccountingDate.HasValue
                ? candidate.LastAccountingDate.Value.Date.AddDays(1)
                : new DateTime(now.Year, 1, 1);

            return new KontoAfslutArealSagerLetterMergeData(
                candidate.CaseNumber,
                candidate.AccountNumber,
                candidate.PropertyAddress,
                FormatLongDate(now),
                FormatLongDate(fromDate),
                FormatLongDate(candidate.CreatedOn.Date),
                candidate.ContactName,
                candidate.AddressLine1,
                candidate.PostalCode,
                candidate.City);
        }

        private static string FormatLongDate(DateTime value)
        {
            CultureInfo culture = new CultureInfo("da-DK");
            return value.ToString("dddd 'den' d. MMMM yyyy", culture);
        }
    }
}
