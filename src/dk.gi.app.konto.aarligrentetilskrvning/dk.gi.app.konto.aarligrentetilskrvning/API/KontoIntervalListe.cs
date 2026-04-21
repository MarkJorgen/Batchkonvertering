using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app.konto.aarligrentetilskrvning
{
    public class KontoIntervalListe
    {
        public List<KontoInterval> Hent()
        {
            List<KontoInterval> kontoIntervalListe = new List<KontoInterval>();

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(KontoInterval[]));

            if (File.Exists(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\API\\KontoInterval.json"))
            {
                try
                {
                    using (FileStream fileStream = new FileStream(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\API\\KontoInterval.json", System.IO.FileMode.Open))
                    {
                        kontoIntervalListe = ((KontoInterval[])serializer.ReadObject(fileStream)).ToList();
                    }
                }
                catch
                {
                    throw new Exception("Kunne ikke indlæse kontointerval liste");
                }
            }

            return kontoIntervalListe;
        }
    }
}
