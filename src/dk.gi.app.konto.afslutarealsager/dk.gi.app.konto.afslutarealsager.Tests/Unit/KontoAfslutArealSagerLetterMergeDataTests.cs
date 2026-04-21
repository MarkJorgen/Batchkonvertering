using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.konto.afslutarealsager.Application.Models;

namespace dk.gi.app.konto.afslutarealsager.Tests.Unit
{
    [TestClass]
    public class KontoAfslutArealSagerLetterMergeDataTests
    {
        [TestMethod]
        public void Create_Uses_LastAccountingDate_Plus_One_Day_As_FraDato()
        {
            var candidate = new KontoAfslutArealSagerCandidate(
                Guid.NewGuid(),
                "SAG-1",
                new DateTime(2026, 4, 17),
                Guid.NewGuid(),
                "41-00001",
                Guid.NewGuid(),
                "Test Person",
                "0101701234",
                string.Empty,
                "Testvej 1",
                "2100",
                "København Ø",
                Guid.NewGuid(),
                "Ejendomsvej 10",
                new DateTime(2025, 12, 31));

            var merge = KontoAfslutArealSagerLetterMergeData.Create(candidate, new DateTime(2026, 4, 17));

            Assert.AreEqual("torsdag den 1. januar 2026", merge.FraDato);
            Assert.AreEqual("fredag den 17. april 2026", merge.TilDato);
            Assert.AreEqual("41-00001", merge.Kontonr);
        }
    }
}
