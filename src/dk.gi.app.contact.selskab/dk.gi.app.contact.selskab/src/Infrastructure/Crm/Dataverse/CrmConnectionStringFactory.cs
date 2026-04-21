using dk.gi.app.contact.selskab.Application.Models;

namespace dk.gi.app.contact.selskab.Infrastructure.Crm
{
    public static class CrmConnectionStringFactory
    {
        public static string Create(ContactSelskabSettings settings)
        {
            return Gi.Batch.Shared.Crm.CrmConnectionStringFactory.Create(
                settings?.CrmConnectionTemplate,
                settings?.CrmServerName,
                settings?.CrmClientId,
                settings?.CrmClientSecret,
                settings?.CrmAuthority,
                settings?.CrmAuthorityMode);
        }

        public static string CreateSanitized(ContactSelskabSettings settings)
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
