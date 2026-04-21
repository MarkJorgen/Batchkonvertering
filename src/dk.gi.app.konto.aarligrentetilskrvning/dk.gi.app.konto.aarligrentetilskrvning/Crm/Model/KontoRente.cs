using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app.konto.aarligrentetilskrvning
{
    public class KontoRenteLinje
    {
        public Guid KontoId { get; set; }

        public string KontoNr { get; set; }

        public decimal Rente { get; set; }
    }
}
