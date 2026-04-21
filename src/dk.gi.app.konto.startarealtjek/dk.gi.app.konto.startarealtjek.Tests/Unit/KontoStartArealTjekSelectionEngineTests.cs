using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.konto.startarealtjek.Application.Models;
using dk.gi.app.konto.startarealtjek.Application.Services;

namespace dk.gi.app.konto.startarealtjek.Tests.Unit
{
    [TestClass]
    public class KontoStartArealTjekSelectionEngineTests
    {
        [TestMethod]
        public void SelectCandidates_Begrnser_Antal_Pr_Ejendomstype_Og_Sorterer_Null_Frst()
        {
            var engine = new KontoStartArealTjekSelectionEngine();
            var assessments = new[]
            {
                CreateAssessment("41-00001", KontoStartArealTjekPropertyType.AlmindeligUdlejning, true, null),
                CreateAssessment("41-00002", KontoStartArealTjekPropertyType.AlmindeligUdlejning, true, new DateTime(2024, 1, 1)),
                CreateAssessment("41-00003", KontoStartArealTjekPropertyType.AlmindeligUdlejning, true, new DateTime(2020, 1, 1)),
                CreateAssessment("51-00001", KontoStartArealTjekPropertyType.Ejerforening, true, null),
                CreateAssessment("61-00001", KontoStartArealTjekPropertyType.Andelsbolig, false, null)
            };

            var batch = new KontoStartArealTjekBatchSettings(3, 1970, 2, 1, 1, "test");
            var result = engine.SelectCandidates(assessments, batch).ToArray();

            Assert.AreEqual(3, result.Length);
            CollectionAssert.AreEqual(new[] { "41-00001", "41-00003", "51-00001" }, result.Select(x => x.AccountNumber).ToArray());
        }

        private static KontoStartArealTjekAssessment CreateAssessment(string kontoNr, KontoStartArealTjekPropertyType propertyType, bool shouldBeSubject, DateTime? lastArealCheck)
        {
            var account = new KontoStartArealTjekAccount(Guid.NewGuid(), kontoNr, propertyType, "1965", 1965, false, lastArealCheck);
            return new KontoStartArealTjekAssessment(account, false, false, shouldBeSubject);
        }
    }
}
