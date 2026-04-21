namespace dk.gi.app.konto.regnskab.slet.Application.Models
{
    public sealed class ExecutionReport
    {
        public bool ConnectivityVerified { get; set; }
        public int SelectedAccountCount { get; set; }
        public int PublishedCount { get; set; }
    }
}
