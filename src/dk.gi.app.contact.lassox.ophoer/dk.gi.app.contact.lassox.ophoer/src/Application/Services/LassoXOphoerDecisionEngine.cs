using System;
using System.Collections.Generic;
using System.Linq;
using dk.gi.app.contact.lassox.ophoer.Application.Models;

namespace dk.gi.app.contact.lassox.ophoer.Application.Services
{
    public sealed class LassoXOphoerDecisionEngine
    {
        public IReadOnlyCollection<LassoXOphoerDecision> Decide(
            IReadOnlyCollection<LassoXContactCandidate> contacts,
            IReadOnlyCollection<Guid> openAccountOwnerContactIds,
            IReadOnlyCollection<Guid> realOwnerContactIds)
        {
            var accountOwnerIds = new HashSet<Guid>(openAccountOwnerContactIds ?? Array.Empty<Guid>());
            var ownerIds = new HashSet<Guid>(realOwnerContactIds ?? Array.Empty<Guid>());
            var result = new List<LassoXOphoerDecision>();

            foreach (var contact in contacts ?? Array.Empty<LassoXContactCandidate>())
            {
                if (accountOwnerIds.Contains(contact.ContactId))
                {
                    result.Add(new LassoXOphoerDecision(contact.ContactId, contact.FullName, true, "Kontakten er åben kontoejer."));
                    continue;
                }

                if (ownerIds.Contains(contact.ContactId))
                {
                    result.Add(new LassoXOphoerDecision(contact.ContactId, contact.FullName, true, "Kontakten er aktiv reel ejer."));
                    continue;
                }

                result.Add(new LassoXOphoerDecision(contact.ContactId, contact.FullName, false, "Kontakten findes ikke længere som åben kontoejer eller aktiv reel ejer."));
            }

            return result;
        }
    }
}
