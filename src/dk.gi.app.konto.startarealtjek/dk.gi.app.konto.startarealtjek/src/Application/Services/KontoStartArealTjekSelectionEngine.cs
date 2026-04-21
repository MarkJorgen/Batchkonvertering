using System;
using System.Collections.Generic;
using System.Linq;
using dk.gi.app.konto.startarealtjek.Application.Models;

namespace dk.gi.app.konto.startarealtjek.Application.Services
{
    public sealed class KontoStartArealTjekSelectionEngine
    {
        public IReadOnlyCollection<KontoStartArealTjekCandidate> SelectCandidates(IReadOnlyCollection<KontoStartArealTjekAssessment> assessments, KontoStartArealTjekBatchSettings batchSettings)
        {
            assessments = assessments ?? Array.Empty<KontoStartArealTjekAssessment>();
            batchSettings = batchSettings ?? new KontoStartArealTjekBatchSettings(0, 1970, 0, 0, 0, "none");

            var result = new List<KontoStartArealTjekCandidate>();
            result.AddRange(SelectForType(assessments, KontoStartArealTjekPropertyType.AlmindeligUdlejning, batchSettings.BatchCountAlmindeligUdlejning));
            result.AddRange(SelectForType(assessments, KontoStartArealTjekPropertyType.Ejerforening, batchSettings.BatchCountEjerforening));
            result.AddRange(SelectForType(assessments, KontoStartArealTjekPropertyType.Andelsbolig, batchSettings.BatchCountAndelsbolig));
            return result;
        }

        private static IEnumerable<KontoStartArealTjekCandidate> SelectForType(IReadOnlyCollection<KontoStartArealTjekAssessment> assessments, KontoStartArealTjekPropertyType propertyType, int batchCount)
        {
            if (batchCount <= 0)
            {
                return Array.Empty<KontoStartArealTjekCandidate>();
            }

            return assessments
                .Where(x => x != null && x.Account != null && x.ShouldBeSubject && x.Account.PropertyType == propertyType)
                .OrderBy(x => x.Account.LastArealCheckUtc.HasValue ? 1 : 0)
                .ThenBy(x => x.Account.LastArealCheckUtc ?? DateTime.MinValue)
                .ThenBy(x => x.Account.AccountNumber, StringComparer.OrdinalIgnoreCase)
                .Take(batchCount)
                .Select(x => new KontoStartArealTjekCandidate(x.Account.AccountId, x.Account.AccountNumber, x.Account.PropertyType, x.Account.LastArealCheckUtc))
                .ToList();
        }
    }
}
