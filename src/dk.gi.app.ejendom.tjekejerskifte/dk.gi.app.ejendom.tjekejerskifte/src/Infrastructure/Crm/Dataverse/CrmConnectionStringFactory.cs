using dk.gi.app.ejendom.tjekejerskifte.Application.Models;

namespace dk.gi.app.ejendom.tjekejerskifte.Infrastructure.Crm.Dataverse
{
    public static class CrmConnectionStringFactory
    {
        public static string Create(EjendomTjekEjerskifteSettings settings)
        {
            return Gi.Batch.Shared.Crm.CrmConnectionStringFactory.Create(
                settings?.CrmConnectionTemplate,
                settings?.CrmServerName,
                settings?.CrmClientId,
                settings?.CrmClientSecret,
                settings?.CrmAuthority,
                settings?.CrmAuthorityMode);
        }

        public static string CreateSanitized(EjendomTjekEjerskifteSettings settings)
        {
            return Gi.Batch.Shared.Crm.CrmConnectionStringFactory.CreateSanitized(
                settings?.CrmConnectionTemplate,
                settings?.CrmServerName,
                settings?.CrmClientId,
                settings?.CrmClientSecret,
                settings?.CrmAuthority,
                settings?.CrmAuthorityMode);
        }
    }
}
