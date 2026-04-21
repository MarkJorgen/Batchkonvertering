using System;
using System.IO;
using Aspose.Words;
using dk.gi.app.konto.afslutarealsager.Application.Abstractions;
using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Documents
{
    public sealed class ArealLukLetterGenerator : ILetterGenerator
    {
        public byte[] GeneratePdf(KontoAfslutArealSagerLetterMergeData mergeData)
        {
            if (mergeData == null) throw new ArgumentNullException(nameof(mergeData));
            ApplyLicense();
            using (Stream templateStream = OpenEmbeddedResource("Skabeloner.LukSmaaArealBrev.doc"))
            {
                var document = new Document(templateStream);
                document.MailMerge.Execute(
                    new[] { "Sagsnr", "Kontonr", "Ejendom", "DAGSDATO", "FraDato", "TilDato", "Navn", "Adresse", "Postnr", "By" },
                    new object[] { mergeData.Sagsnr, mergeData.Kontonr, mergeData.Ejendom, mergeData.DagsDato, mergeData.FraDato, mergeData.TilDato, mergeData.Navn, mergeData.Adresse, mergeData.Postnr, mergeData.By });

                using (var output = new MemoryStream())
                {
                    document.Save(output, SaveFormat.Pdf);
                    return output.ToArray();
                }
            }
        }

        private static void ApplyLicense()
        {
            using (Stream licenseStream = OpenEmbeddedResource("Aspose.2016_Aspose.Total.lic"))
            {
                var license = new License();
                license.SetLicense(licenseStream);
            }
        }

        private static Stream OpenEmbeddedResource(string suffix)
        {
            var assembly = typeof(ArealLukLetterGenerator).Assembly;
            string resourceName = assembly.GetName().Name + "." + suffix;
            Stream stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new InvalidOperationException("Embedded resource blev ikke fundet: " + resourceName);
            return stream;
        }
    }
}
