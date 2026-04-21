namespace dk.gi.app.konto.afslutarealsager.Application.Models
{
    public sealed class KontoAfslutArealSagerArealCarryForwardResult
    {
        public bool Attempted { get; }
        public bool ClosedExistingArea { get; }
        public bool CreatedNewArea { get; }
        public bool DeletedZeroRegnskab { get; }
        public string AccountNumber { get; }
        public string Message { get; }
        public string NewAreaId { get; }

        public KontoAfslutArealSagerArealCarryForwardResult(
            bool attempted,
            bool closedExistingArea,
            bool createdNewArea,
            bool deletedZeroRegnskab,
            string accountNumber,
            string message,
            string newAreaId)
        {
            Attempted = attempted;
            ClosedExistingArea = closedExistingArea;
            CreatedNewArea = createdNewArea;
            DeletedZeroRegnskab = deletedZeroRegnskab;
            AccountNumber = accountNumber ?? string.Empty;
            Message = message ?? string.Empty;
            NewAreaId = newAreaId ?? string.Empty;
        }

        public static KontoAfslutArealSagerArealCarryForwardResult Skipped(string accountNumber, string message)
            => new KontoAfslutArealSagerArealCarryForwardResult(false, false, false, false, accountNumber, message, string.Empty);

        public static KontoAfslutArealSagerArealCarryForwardResult Completed(string accountNumber, bool deletedZeroRegnskab, string newAreaId, string message)
            => new KontoAfslutArealSagerArealCarryForwardResult(true, true, true, deletedZeroRegnskab, accountNumber, message, newAreaId);
    }
}
