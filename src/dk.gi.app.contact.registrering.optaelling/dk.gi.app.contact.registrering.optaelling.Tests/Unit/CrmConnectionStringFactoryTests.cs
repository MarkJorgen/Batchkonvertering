using dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace dk.gi.app.contact.registrering.optaelling.Tests.Unit
{
    [TestClass]
    public class CrmConnectionStringFactoryTests
    {
        [TestMethod]
        public void Create_Formats_Connection_String_From_Settings()
        {
            var settings = ContactRegistreringOptaellingSettingsBuilder.Build(
                mode: "RUN",
                crmConnectionTemplate: "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};",
                crmServerName: "server.crm4.dynamics.com",
                crmClientId: "client-id",
                crmClientSecret: "plain-secret",
                crmAuthority: "login.microsoftonline.com/tenant");

            string connectionString = CrmConnectionStringFactory.Create(settings);

            StringAssert.Contains(connectionString, "server.crm4.dynamics.com");
            StringAssert.Contains(connectionString, "client-id");
            StringAssert.Contains(connectionString, "login.microsoftonline.com/tenant");
            Assert.IsFalse(string.IsNullOrWhiteSpace(connectionString));
        }

        [TestMethod]
        public void Create_TenantBase_Rewrites_Authority_To_Tenant_Base()
        {
            var settings = ContactRegistreringOptaellingSettingsBuilder.Build(
                mode: "VERIFYCRM",
                crmConnectionTemplate: "AuthType=ClientSecret;Url=https://{0}/;Clientid={1};ClientSecret={2};Authority=https://login.microsoftonline.com/{3}/oauth2/v2.0/authorize;RequireNewInstance=True;",
                crmServerName: "gicrmdev.crm4.dynamics.com",
                crmClientId: "client-id",
                crmClientSecret: "plain-secret",
                crmAuthority: "d5356f0d-2d9d-4c6c-86ed-f15d0c7f72e7",
                crmAuthorityMode: "TenantBase");

            string connectionString = CrmConnectionStringFactory.Create(settings);

            StringAssert.Contains(connectionString, "Authority=https://login.microsoftonline.com/d5356f0d-2d9d-4c6c-86ed-f15d0c7f72e7;");
            Assert.IsFalse(connectionString.Contains("/oauth2/v2.0/authorize"));
        }

        [TestMethod]
        public void Create_Omit_Removes_Authority_From_Connection_String()
        {
            var settings = ContactRegistreringOptaellingSettingsBuilder.Build(
                mode: "VERIFYCRM",
                crmConnectionTemplate: "AuthType=ClientSecret;Url=https://{0}/;Clientid={1};ClientSecret={2};Authority=https://login.microsoftonline.com/{3}/oauth2/v2.0/authorize;RequireNewInstance=True;",
                crmServerName: "gicrmdev.crm4.dynamics.com",
                crmClientId: "client-id",
                crmClientSecret: "plain-secret",
                crmAuthority: "d5356f0d-2d9d-4c6c-86ed-f15d0c7f72e7",
                crmAuthorityMode: "Omit");

            string connectionString = CrmConnectionStringFactory.Create(settings);

            Assert.IsFalse(connectionString.Contains("Authority="));
            StringAssert.Contains(connectionString, "RequireNewInstance=True");
        }
        [TestMethod]
        public void Create_Decrypts_Compat_Encrypted_Client_Secret()
        {
            string encryptedSecret = EncryptCompat("plain-secret");

            var settings = ContactRegistreringOptaellingSettingsBuilder.Build(
                mode: "RUN",
                crmConnectionTemplate: "AuthType=ClientSecret;Url=https://{0};ClientId={1};ClientSecret={2};Authority=https://{3};",
                crmServerName: "server.crm4.dynamics.com",
                crmClientId: "client-id",
                crmClientSecret: encryptedSecret,
                crmAuthority: "login.microsoftonline.com/tenant");

            string connectionString = CrmConnectionStringFactory.Create(settings);

            StringAssert.Contains(connectionString, "ClientSecret=plain-secret");
        }

        private static string EncryptCompat(string value)
        {
            const int encryptionKey = 17;
            char[] transformed = new char[value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                transformed[i] = (char)(value[i] ^ encryptionKey);
            }

            byte[] bytes = new byte[transformed.Length * sizeof(char)];
            System.Buffer.BlockCopy(transformed, 0, bytes, 0, bytes.Length);
            return System.Convert.ToBase64String(bytes);
        }
    }
}
