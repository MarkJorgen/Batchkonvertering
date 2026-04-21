namespace dk.gi.app.konto.kontoejerLuk.Application.Models
{
    public sealed class KontoejerLukExecutionSummary
    {
        public bool Success { get; }
        public int ScannedAccounts { get; }
        public int OpenOwners { get; }
        public int UpdatedOwners { get; }
        public string Message { get; }
        public string Source { get; }

        public KontoejerLukExecutionSummary(bool success, int scannedAccounts, int openOwners, int updatedOwners, string message, string source)
        {
            Success = success;
            ScannedAccounts = scannedAccounts;
            OpenOwners = openOwners;
            UpdatedOwners = updatedOwners;
            Message = message ?? string.Empty;
            Source = source ?? string.Empty;
        }
    }
}
