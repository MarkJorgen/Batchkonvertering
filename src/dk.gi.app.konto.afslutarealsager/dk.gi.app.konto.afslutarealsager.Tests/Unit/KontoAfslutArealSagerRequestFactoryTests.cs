using System.Collections.Generic;
using dk.gi.app.konto.afslutarealsager.Application.Models;
using dk.gi.app.konto.afslutarealsager.Infrastructure.Config;
using Gi.Batch.Shared.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.konto.afslutarealsager.Tests.Unit
{
    [TestClass]
    public class KontoAfslutArealSagerRequestFactoryTests
    {
        [TestMethod]
        public void Create_Maps_Optional_Force_Filters()
        {
            var configuration = new JobConfiguration(new Dictionary<string, string>
            {
                ["Mode"] = "DRYRUN",
                ["CrmConnectionTemplate"] = "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};",
                ["CrmServerName"] = "server",
                ["CrmClientId"] = "client",
                ["CrmClientSecret"] = "secret",
                ["CrmAuthority"] = "tenant",
                ["BrugerArealSager"] = "sma@gi.dk",
                ["ForceIncidentId"] = "11111111-1111-1111-1111-111111111111",
                ["ForceSagsnummer"] = "SAG-42",
                ["ForceKontonr"] = "45-00001"
            });

            KontoAfslutArealSagerSettings settings = KontoAfslutArealSagerSettings.Create(configuration);
            KontoAfslutArealSagerRequest request = KontoAfslutArealSagerRequestFactory.Create(configuration, settings);

            Assert.AreEqual("11111111-1111-1111-1111-111111111111", request.ForceIncidentId);
            Assert.AreEqual("SAG-42", request.ForceSagsnummer);
            Assert.AreEqual("45-00001", request.ForceKontonr);
            Assert.IsTrue(request.HasForcedCaseSelector);
            Assert.IsTrue(request.HasForcedAccountSelector);
        }
    }
}
