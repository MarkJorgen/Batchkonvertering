using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app.konto.aarligrentetilskrvning
{
    public class KontoLinje
    {
        public Guid KontoId { get; set; }
        public string KontoNr { get; set; }
        public decimal Kontoindestaaende { get; set; }
    }
}
