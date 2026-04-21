using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app.konto.aarligrentetilskrvning
{
    public class KontiMedtages
    {
        public List<string> Hent(ILogger trace)
        {
            List<string> kontoListe = new List<string>();

            if (File.Exists(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\API\\KontiMedtages.csv"))
            {
                try
                {
                    trace.LogInformation("Henter kontimedtages...");

                    int taeller = 0;
                    string linje;
                    System.IO.StreamReader file = new System.IO.StreamReader(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\API\\KontiMedtages.csv");

                    while ((linje = file.ReadLine()) != null)
                    {
                        kontoListe.Add(linje);
                        taeller++;
                    }

                    file.Close();

                    trace.LogInformation($"Hentet kontimedtages {taeller}...");
                }
                catch
                {
                    throw new Exception("Kunne ikke indlæse kontoListe");
                }
            }

            return kontoListe;
        }
    }
}
