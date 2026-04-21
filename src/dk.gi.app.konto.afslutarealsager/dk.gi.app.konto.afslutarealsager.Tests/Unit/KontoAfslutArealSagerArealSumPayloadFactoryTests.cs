using dk.gi.app.konto.afslutarealsager.Application.Models;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace dk.gi.app.konto.afslutarealsager.Tests.Unit
{
    [TestClass]
    public class KontoAfslutArealSagerArealSumPayloadFactoryTests
    {
        [TestMethod]
        public void Create_Contains_Legacy_ArealSum_Keys()
        {
            var candidate = new KontoAfslutArealSagerCandidate(
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                "SAG-42",
                new DateTime(2026, 4, 17),
                Guid.Parse("22222222-2222-2222-2222-222222222222"),
                "45-00001",
                Guid.Parse("33333333-3333-3333-3333-333333333333"),
                "Test Testsen",
                "0101701234",
                string.Empty,
                "Testvej 1",
                "2100",
                "København Ø",
                Guid.Parse("44444444-4444-4444-4444-444444444444"),
                "Ejendomsgade 2",
                new DateTime(2025, 12, 31));

            string payload = KontoAfslutArealSagerArealSumPayloadFactory.Create(candidate, "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

            StringAssert.Contains(payload, "\"Key\":\"action\",\"Value\":\"UPDATE\"");
            StringAssert.Contains(payload, "\"Key\":\"logicalname\",\"Value\":\"ap_areal\"");
            StringAssert.Contains(payload, "\"Key\":\"kontonr\",\"Value\":\"45-00001\"");
        }
    }
}
