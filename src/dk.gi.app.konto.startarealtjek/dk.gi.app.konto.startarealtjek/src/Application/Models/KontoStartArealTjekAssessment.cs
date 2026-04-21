namespace dk.gi.app.konto.startarealtjek.Application.Models
{
    public sealed class KontoStartArealTjekAssessment
    {
        public KontoStartArealTjekAccount Account { get; }
        public bool HasStatusCode22 { get; }
        public bool HasOpenCases { get; }
        public bool ShouldBeSubject { get; }
        public bool WillUpdateSubjectFlag { get; }

        public KontoStartArealTjekAssessment(
            KontoStartArealTjekAccount account,
            bool hasStatusCode22,
            bool hasOpenCases,
            bool shouldBeSubject)
        {
            Account = account;
            HasStatusCode22 = hasStatusCode22;
            HasOpenCases = hasOpenCases;
            ShouldBeSubject = shouldBeSubject;
            WillUpdateSubjectFlag = account != null && account.ExistingIsSubject.GetValueOrDefault(false) != shouldBeSubject;
        }
    }
}
