using System.Collections.Generic;
using System.Linq;
using dk.gi.app.contact.selskab.Application.Models;

namespace dk.gi.app.contact.selskab.Application.Services
{
    public sealed class ContactSelskabSelectionEngine
    {
        public IReadOnlyCollection<ContactSelskabCandidate> SelectCandidates(IReadOnlyCollection<ContactSelskabOwnerObservation> observations)
        {
            if (observations == null || observations.Count == 0)
            {
                return new List<ContactSelskabCandidate>();
            }

            return observations
                .Where(x => x.CompanyId != System.Guid.Empty)
                .Where(x => string.IsNullOrWhiteSpace(x.CvrNumber) == false)
                .GroupBy(x => x.CvrNumber.Trim())
                .Select(group =>
                {
                    int yesCount = group.Count(x => x.UltimateOwnerHasKdk);
                    int noCount = group.Count(x => !x.UltimateOwnerHasKdk);
                    var first = group.First();
                    return new ContactSelskabCandidate(first.CompanyId, group.Key, yesCount, noCount);
                })
                .Where(x => x.OwnersWithKdkYes >= 1 && x.OwnersWithKdkNo <= 0)
                .OrderBy(x => x.CvrNumber)
                .ToList();
        }
    }
}
