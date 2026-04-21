namespace dk.gi.app.konto.startarealtjek.Application.Models
{
    public sealed class KontoStartArealTjekBatchSettings
    {
        public int ArealCheckYears { get; }
        public int BuildYearBefore { get; }
        public int BatchCountAlmindeligUdlejning { get; }
        public int BatchCountEjerforening { get; }
        public int BatchCountAndelsbolig { get; }
        public string Source { get; }

        public KontoStartArealTjekBatchSettings(int arealCheckYears, int buildYearBefore, int batchCountAlmindeligUdlejning, int batchCountEjerforening, int batchCountAndelsbolig, string source)
        {
            ArealCheckYears = arealCheckYears;
            BuildYearBefore = buildYearBefore;
            BatchCountAlmindeligUdlejning = batchCountAlmindeligUdlejning;
            BatchCountEjerforening = batchCountEjerforening;
            BatchCountAndelsbolig = batchCountAndelsbolig;
            Source = source ?? string.Empty;
        }
    }
}
