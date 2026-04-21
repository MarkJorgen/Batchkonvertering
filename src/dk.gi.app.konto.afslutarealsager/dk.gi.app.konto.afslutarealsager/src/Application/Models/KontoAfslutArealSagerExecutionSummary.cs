namespace dk.gi.app.konto.afslutarealsager.Application.Models
{
    public sealed class KontoAfslutArealSagerExecutionSummary
    {
        public bool Success { get; }
        public string Source { get; }
        public string Message { get; }
        public int ScannedCases { get; }
        public int LetterCandidates { get; }
        public int GeneratedLetters { get; }
        public int SkippedCases { get; }
        public int CreatedActivities { get; }
        public int UploadedLetters { get; }
        public int CompletedActivities { get; }
        public int PublishedCloseoutJobs { get; }
        public int ClosedIncidents { get; }
        public int ClosedAreas { get; }
        public int CreatedAreas { get; }
        public int DeletedZeroRegnskaber { get; }
        public int PublishedArealSumJobs { get; }
        public int StagedDigitalPosts { get; }
        public bool PartialRunBlocked { get; }

        public KontoAfslutArealSagerExecutionSummary(
            bool success,
            string source,
            string message,
            int scannedCases,
            int letterCandidates,
            int generatedLetters,
            int skippedCases,
            int createdActivities,
            int uploadedLetters,
            int completedActivities,
            int publishedCloseoutJobs,
            int closedIncidents,
            int closedAreas,
            int createdAreas,
            int deletedZeroRegnskaber,
            int publishedArealSumJobs,
            int stagedDigitalPosts,
            bool partialRunBlocked)
        {
            Success = success;
            Source = source ?? string.Empty;
            Message = message ?? string.Empty;
            ScannedCases = scannedCases;
            LetterCandidates = letterCandidates;
            GeneratedLetters = generatedLetters;
            SkippedCases = skippedCases;
            CreatedActivities = createdActivities;
            UploadedLetters = uploadedLetters;
            CompletedActivities = completedActivities;
            PublishedCloseoutJobs = publishedCloseoutJobs;
            ClosedIncidents = closedIncidents;
            ClosedAreas = closedAreas;
            CreatedAreas = createdAreas;
            DeletedZeroRegnskaber = deletedZeroRegnskaber;
            PublishedArealSumJobs = publishedArealSumJobs;
            StagedDigitalPosts = stagedDigitalPosts;
            PartialRunBlocked = partialRunBlocked;
        }

        public static KontoAfslutArealSagerExecutionSummary Ok(
            string source,
            string message,
            int scannedCases = 0,
            int letterCandidates = 0,
            int generatedLetters = 0,
            int skippedCases = 0,
            int createdActivities = 0,
            int uploadedLetters = 0,
            int completedActivities = 0,
            int publishedCloseoutJobs = 0,
            int closedIncidents = 0,
            int closedAreas = 0,
            int createdAreas = 0,
            int deletedZeroRegnskaber = 0,
            int publishedArealSumJobs = 0,
            int stagedDigitalPosts = 0)
            => new KontoAfslutArealSagerExecutionSummary(true, source, message, scannedCases, letterCandidates, generatedLetters, skippedCases, createdActivities, uploadedLetters, completedActivities, publishedCloseoutJobs, closedIncidents, closedAreas, createdAreas, deletedZeroRegnskaber, publishedArealSumJobs, stagedDigitalPosts, false);

        public static KontoAfslutArealSagerExecutionSummary Fail(string source, string message, bool partialRunBlocked = false)
            => new KontoAfslutArealSagerExecutionSummary(false, source, message, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, partialRunBlocked);
    }
}
