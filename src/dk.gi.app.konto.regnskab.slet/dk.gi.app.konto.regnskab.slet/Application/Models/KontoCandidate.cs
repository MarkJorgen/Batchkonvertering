using System;

namespace dk.gi.app.konto.regnskab.slet.Application.Models
{
    public sealed class KontoCandidate
    {
        public Guid AccountId { get; set; }
        public string AccountNumber { get; set; }
    }
}
