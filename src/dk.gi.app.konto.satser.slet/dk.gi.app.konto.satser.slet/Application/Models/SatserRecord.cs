using System;

namespace dk.gi.app.konto.satser.slet.Application.Models
{
    public sealed class SatserRecord
    {
        public Guid Id { get; set; }

        public DateTime? StartdatoLocal { get; set; }

        public bool IsUndtagelse { get; set; }
    }
}
