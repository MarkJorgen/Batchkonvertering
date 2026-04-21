using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.contact.lassox.ophoer.Application.Models;
using dk.gi.app.contact.lassox.ophoer.Application.Services;

namespace dk.gi.app.contact.lassox.ophoer.Tests.Unit
{
    [TestClass]
    public class LassoXOphoerDecisionEngineTests
    {
        [TestMethod]
        public void Decide_KontaktErKontoejer_BeholderAbonnement()
        {
            var engine = new LassoXOphoerDecisionEngine();
            var contactId = Guid.NewGuid();

            var decisions = engine.Decide(
                new[] { new LassoXContactCandidate(contactId, "Test Testsen") },
                new[] { contactId },
                Array.Empty<Guid>());

            Assert.AreEqual(1, decisions.Count);
            Assert.IsTrue(System.Linq.Enumerable.First(decisions).KeepSubscription);
        }

        [TestMethod]
        public void Decide_KontaktErReelEjer_BeholderAbonnement()
        {
            var engine = new LassoXOphoerDecisionEngine();
            var contactId = Guid.NewGuid();

            var decisions = engine.Decide(
                new[] { new LassoXContactCandidate(contactId, "Test Testsen") },
                Array.Empty<Guid>(),
                new[] { contactId });

            Assert.IsTrue(System.Linq.Enumerable.First(decisions).KeepSubscription);
        }

        [TestMethod]
        public void Decide_KontaktErIkkeEjer_AfmelderAbonnement()
        {
            var engine = new LassoXOphoerDecisionEngine();
            var contactId = Guid.NewGuid();

            var decisions = engine.Decide(
                new[] { new LassoXContactCandidate(contactId, "Test Testsen") },
                Array.Empty<Guid>(),
                Array.Empty<Guid>());

            Assert.IsFalse(System.Linq.Enumerable.First(decisions).KeepSubscription);
        }
    }
}
