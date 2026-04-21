using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.contact.selskab.Application.Models;
using dk.gi.app.contact.selskab.Application.Services;

namespace dk.gi.app.contact.selskab.Tests.Unit
{
    [TestClass]
    public class ContactSelskabSelectionEngineTests
    {
        [TestMethod]
        public void SelectCandidates_AlleUltimativeEjereJa_ReturnererSelskab()
        {
            var engine = new ContactSelskabSelectionEngine();
            var companyId = Guid.NewGuid();

            var result = engine.SelectCandidates(new[]
            {
                new ContactSelskabOwnerObservation(companyId, "12345678", true),
                new ContactSelskabOwnerObservation(companyId, "12345678", true)
            });

            Assert.AreEqual(1, result.Count);
            var candidate = System.Linq.Enumerable.First(result);
            Assert.AreEqual(companyId, candidate.CompanyId);
            Assert.AreEqual(2, candidate.OwnersWithKdkYes);
            Assert.AreEqual(0, candidate.OwnersWithKdkNo);
        }

        [TestMethod]
        public void SelectCandidates_BlandetJaOgNej_UdeladerSelskab()
        {
            var engine = new ContactSelskabSelectionEngine();
            var companyId = Guid.NewGuid();

            var result = engine.SelectCandidates(new[]
            {
                new ContactSelskabOwnerObservation(companyId, "12345678", true),
                new ContactSelskabOwnerObservation(companyId, "12345678", false)
            });

            Assert.AreEqual(0, result.Count);
        }
    }
}
