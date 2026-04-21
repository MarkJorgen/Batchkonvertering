using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using dk.gi.app.konto.startarealtjek.Application.Models;
using dk.gi.app.konto.startarealtjek.Infrastructure.Messaging;

namespace dk.gi.app.konto.startarealtjek.Tests.Unit
{
    [TestClass]
    public class KontoStartArealTjekPayloadFactoryTests
    {
        [TestMethod]
        public void Create_Builds_Expected_Action_Id_And_Kontonr()
        {
            var accountId = Guid.Parse("11111111-2222-3333-4444-555555555555");
            var candidate = new KontoStartArealTjekCandidate(accountId, "41-00001", KontoStartArealTjekPropertyType.AlmindeligUdlejning, null);

            string payload = KontoStartArealTjekPayloadFactory.Create(candidate);

            StringAssert.Contains(payload, "AREALCHECKSAG");
            StringAssert.Contains(payload, "Batch:dk.gi.app.konto.startarealtjek");
            StringAssert.Contains(payload, accountId.ToString("D").ToUpperInvariant());
            StringAssert.Contains(payload, "41-00001");
        }
    }
}
