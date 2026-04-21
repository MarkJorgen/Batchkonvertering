using System;

namespace dk.gi.app.konto.beregnsatserlog.slet.Application.Models
{
    public sealed class BeregnSatserLogRecord
    {
        public Guid Id { get; set; }

        public DateTime? CreatedOnUtc { get; set; }
    }
}
