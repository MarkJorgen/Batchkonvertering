using System;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Application.Models
{
    public sealed class RunoutRegistreringCandidate
    {
        public Guid Id { get; }
        public string Sagsnr { get; }
        public Guid? TreklipId { get; }

        public RunoutRegistreringCandidate(Guid id, string sagsnr, Guid? treklipId)
        {
            Id = id;
            Sagsnr = sagsnr ?? string.Empty;
            TreklipId = treklipId;
        }
    }
}
