using dk.gi.app.contact.registreringudloebne.optaelling.Infrastructure.Crm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registreringudloebne.optaelling.Tests.Unit
{
    [TestClass]
    public class CrmConnectionStringFactoryTests
    {
        [TestMethod]
        public void Create_Applies_TenantBase_Mode()
        {
            var settings = ContactRegistreringUdloebneOptaellingSettingsBuilder.Build("VERIFYCRM", crmAuthority: "login.microsoftonline.com/tenant-id/oauth2/v2.0/authorize", crmAuthorityMode: "TenantBase");
            string result = CrmConnectionStringFactory.Create(settings);
            StringAssert.Contains(result, "Authority=https://login.microsoftonline.com/tenant-id");
        }
    }
}
