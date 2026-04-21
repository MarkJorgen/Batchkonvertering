using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
//
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

// Aspose
using Aspose.Words;
using System.IO;
using System.Xml.Linq;
using System.ComponentModel;
using License = Aspose.Words.License;

namespace dk.gi.crm.app.konto.afslutarealsager
{
    public class pdf
    {
        public pdf()
        {
            // i alle constructor
            Trace = dk.gi.GILoggerProvider.GetLogger(GetType().FullName);
        }

        /// <summary>
        /// En privat logger som default er sat til en NullLogger, den rettes/sættes så i Konstructor
        /// </summary>
        protected ILogger Trace { get; private set; } = NullLogger.Instance;

        /// <summary>
        /// Opretter en forside PDF for en aktivitet.
        /// </summary>
        /// <param name="data">De data der skal bruges (flettes med) under oprettelsen af forsiden.</param>
        /// <returns>En <see cref="System.IO.MemoryStream"/> indeholdende en PDF-fil.</returns>
        internal byte[] DanBrev(Dictionary<string, object> data, List<DataSet> dataSets)
        {
            Trace.LogInformation("Get Aspose License");
            var license = GetAsposeWordsLicense();

            // 2013-10-23 RMP: Opret et Aspose.Words.Document brugt til at oprette/generere forsiden med
            Trace.LogInformation("Get Aspose document");
            Aspose.Words.Document frontPageDocument = GetAsposeWordDocument();

            // 2013-10-23 RMP: Send data til forsiden.
            Trace.LogInformation("MailMerge Execute");
            frontPageDocument.MailMerge.Execute(data.Keys.Select(x => x).ToArray(), data.Values.Select(x => x).ToArray());

            // Initier liste med dataset
            Trace.LogInformation("MailMerge Execute with Regions");
            foreach (DataSet dataset in dataSets)
            {
                frontPageDocument.MailMerge.ExecuteWithRegions(dataset);
            }

            Trace.LogInformation("DanBrev Create memorystream");
            // 2013-10-23 RMP: Opret MemoryStream til at modtage/indeholde resultatet
            var resultStream = new System.IO.MemoryStream();
            Trace.LogInformation("DanBrev save to memorystream");
            // 2013-10-23 RMP: Gem forsiden i resultStream
            frontPageDocument.Save(resultStream, Aspose.Words.SaveFormat.Pdf);

            // 2013-10-23 RMP: Luk/nulstil frontPageDocument
            frontPageDocument = null;

            // 2013-10-23 RMP: returner resultatet
            Trace.LogInformation("Return stream as array");
            return resultStream.ToArray();
        }

        /// <summary>
        /// Get Aspose License from Resource
        /// - Used DotPeek to find RessourceName of License
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Aspose.Words.License GetAsposeWordsLicense()
        {
            Trace.LogInformation("GetAsposeWordsLicense start");
            // Grap current assembly
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            if (assembly == null)
                throw new Exception("Intet resultat fra GetExecutingAssembly");

            // Namespace is part of recource name
            string name = assembly.GetName().Name;
            // Path in source code i also a part of the name [dk.gi.asbq.konto.kontrol.rykbilag.src.Aspose.2016_Aspose.Total.lic]
            name = name + ".Aspose.2016_Aspose.Total.lic";
            Trace.LogInformation($"Search for ResourceStream name:{name}");
            // Get stream
            System.IO.Stream resourceStreamLic = assembly.GetManifestResourceStream(name);
            if (resourceStreamLic == null)
                throw new Exception($"Intet resultat fra GetManifestResourceStream ved navn:{name}");

            // Først indlæs Aspose Word licens fra Assembly Resource
            License lic = new License();
            lic.SetLicense(resourceStreamLic);

            // Create License
            lic = new Aspose.Words.License();

            // yield
            Trace.LogInformation("GetAsposeWordsLicense slut");
            return lic;
        }

        /// <summary>
        /// Get Aspose Word Document from Resource
        /// - Used DotPeek to find RessourceName of Documents
        /// </summary>
        /// <param name="dokument"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private Aspose.Words.Document GetAsposeWordDocument()
        {
            Trace.LogInformation("GetAsposeWordDocument start");
            // Grap current assembly
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            if (assembly == null)
                throw new Exception("Intet resultat fra GetExecutingAssembly");

            // Namespace is part of recource name 
            string name = assembly.GetName().Name;
            // Path in source code i also a part of the name [dk.gi.asbq.konto.kontrol.rykbilag.src.AsposeSkabeloner.Ryk for bilag.docx]
            name = name + ".Skabeloner.LukSmaaArealBrev.doc";
            Trace.LogInformation($"Search for ResourceStream name:{name}");
            // Get stream
            System.IO.Stream resourceStreamLic = assembly.GetManifestResourceStream(name);
            if (resourceStreamLic == null)
                throw new Exception($"Intet resultat fra GetManifestResourceStream ved navn:{name}");

            var frontPageDocument = new Aspose.Words.Document(resourceStreamLic);

            Trace.LogInformation("GetAsposeWordDocument slut");
            return frontPageDocument;
        }
    }
}