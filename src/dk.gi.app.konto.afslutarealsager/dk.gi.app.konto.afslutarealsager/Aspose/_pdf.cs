using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aspose.Pdf.Facades;
using Aspose.Pdf.Generator;
using Aspose.Words;
using Aspose.Cells;

namespace dk.gi.crm.app.konto.afslutarealsager
{
    public class pdf
    {
        /// <summary>
        /// Opretter en forside PDF for en aktivitet.
        /// </summary>
        /// <param name="data">De data der skal bruges (flettes med) under oprettelsen af forsiden.</param>
        /// <returns>En <see cref="System.IO.MemoryStream"/> indeholdende en PDF-fil.</returns>
        internal byte[] DanBrev(Dictionary<string, object> data, string dokument)
        {
            var license = AsposeWordsLicense();

            // 2013-10-23 RMP: Opret MemoryStream til at modtage/indeholde resultatet
            var resultStream = new System.IO.MemoryStream();

            // 2013-10-23 RMP: Opret et Aspose.Words.Document brugt til at oprette/generere forsiden med

            string eksekveringDLL = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string sti = System.IO.Path.GetDirectoryName(eksekveringDLL);

            var frontPageDocument = new Aspose.Words.Document(sti + "\\Skabeloner\\" + dokument);
            // 2013-10-23 RMP: Send data til forsiden.
            frontPageDocument.MailMerge.Execute(data.Keys.Select(x => x).ToArray(), data.Values.Select(x => x).ToArray());
            // 2013-10-23 RMP: Gem forsiden i resultStream
            frontPageDocument.Save(resultStream, Aspose.Words.SaveFormat.Pdf);
            // 2013-10-23 RMP: Luk/nulstil frontPageDocument
            frontPageDocument = null;

            // 2013-10-23 RMP: returner resultatet
            return resultStream.ToArray();
        }

        internal Aspose.Words.License AsposeWordsLicense()
        {
            // Create License
            Aspose.Words.License lic = new Aspose.Words.License();

            // Get assembly
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();

            // Get name of resources -- det er vigtigt at 
            string name = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith("2016_Aspose.Total.lic"));

            if (!String.IsNullOrEmpty(name))
            {
                // Get stream
                System.IO.Stream stream = assembly.GetManifestResourceStream(name);
                // load license by stream
                lic.SetLicense(stream);
            }
            else throw new ApplicationException("Kan ikke loade en Aspose Licens, hverken fra DLL-resurse eller fra fil-systemet");

            // yield
            return lic;
        }
    }

}
