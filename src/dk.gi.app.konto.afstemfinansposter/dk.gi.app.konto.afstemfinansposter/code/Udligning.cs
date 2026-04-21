using dk.gi.crm.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.crm.app.konto.afstemfinansposter
{
    public class Udligning
    {

        public List<AfstemningPostering> Hent(List<AfstemningPostering> posteringer)
        {
            List<AfstemningPostering> crmPosteringer = posteringer.Where(p => p.System == "CRM").ToList();
            List<AfstemningPostering> axPosteringer = posteringer.Where(p => p.System == "AX").ToList();

            List<AfstemningPostering> udlignedePosteringer = new List<AfstemningPostering>();

            foreach (AfstemningPostering postering in crmPosteringer)
            {
                // Findes der 2 posteringer ax/crm der matcher på beløb
                AfstemningPostering match = axPosteringer.Where(p => p.Posteringsdato == postering.Posteringsdato &&
                    p.Beloeb == postering.Beloeb * -1).FirstOrDefault();
                
                if(match != null)
                {
                    udlignedePosteringer.Add(match);
                    udlignedePosteringer.Add(postering);
                }
            }

            // Fjern udlignede ax
            foreach (AfstemningPostering postering in udlignedePosteringer.Where(p => p.System == "AX").ToList())
            {
                axPosteringer.Remove(postering);
            }

            // Fjern udlignede crm
            foreach (AfstemningPostering postering in udlignedePosteringer.Where(p => p.System == "CRM").ToList())
            {
                crmPosteringer.Remove(postering);
            }


            foreach (AfstemningPostering postering in axPosteringer)
            {
                // Findes der 2 posteringer ax/ax der matcher på beløb
                AfstemningPostering match = axPosteringer.Where(p => p.Posteringsdato == postering.Posteringsdato &&
                    p.Beloeb == postering.Beloeb * -1).FirstOrDefault();

                if (match != null)
                {
                    udlignedePosteringer.Add(match);
                    udlignedePosteringer.Add(postering);
                }
            }

            // Fjern udlignede ax
            foreach (AfstemningPostering postering in udlignedePosteringer.Where(p => p.System == "AX").ToList())
            {
                axPosteringer.Remove(postering);
            }

            foreach (AfstemningPostering postering in crmPosteringer)
            {
                // Findes der 2 posteringer crm/crm der matcher på beløb
                AfstemningPostering match = crmPosteringer.Where(p => p.Posteringsdato == postering.Posteringsdato &&
                    p.Beloeb == postering.Beloeb * -1).FirstOrDefault();

                if (match != null)
                {
                    udlignedePosteringer.Add(match);
                    udlignedePosteringer.Add(postering);
                }
            }

            // Fjern udlignede crm
            foreach (AfstemningPostering postering in udlignedePosteringer.Where(p => p.System == "CRM").ToList())
            {
                crmPosteringer.Remove(postering);
            }

            List<AfstemningPostering> resultat = new List<AfstemningPostering>();

            resultat.AddRange(udlignedePosteringer);
            resultat.AddRange(crmPosteringer);
            resultat.AddRange(axPosteringer);

            return resultat;
        }
    }
}
