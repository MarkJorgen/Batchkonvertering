using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace dk.gi.app.konto.aarligrentetilskrvning
{
    public class FinanssaldoLinje
    {
        public Guid Id { get; set; }
        public Guid KontoId { get; set; }
        public decimal AarsRente { get; set; }
    }
}
