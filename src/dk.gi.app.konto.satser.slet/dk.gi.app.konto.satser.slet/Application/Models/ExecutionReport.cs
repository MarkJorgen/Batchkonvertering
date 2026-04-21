namespace dk.gi.app.konto.satser.slet.Application.Models
{
    public sealed class ExecutionReport
    {
        public bool ConnectivityVerified { get; set; }

        public int CandidateCount { get; set; }

        public int DeletedCount { get; set; }
    }
}
