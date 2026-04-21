using dk.gi.app.contact.lassox.ophoer.Application.Models;

namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Crm
{
    public static class CrmConnectionStringFactory
    {
        public static string Create(LassoXOphoerSettings settings)
        {
            return Gi.Batch.Shared.Crm.CrmConnectionStringFactory.Create(
                settings?.CrmConnectionTemplate,
                settings?.CrmServerName,
                settings?.CrmClientId,
                settings?.CrmClientSecret,
                settings?.CrmAuthority,
                settings?.CrmAuthorityMode);
        }

        public static string CreateSanitized(LassoXOphoerSettings settings)
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
