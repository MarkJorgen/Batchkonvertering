using dk.gi.app.contact.registrering.optaelling.Application.Models;

namespace dk.gi.app.contact.registrering.optaelling.Infrastructure.Crm
{
    public static class CrmConnectionStringFactory
    {
        public static string Create(ContactRegistreringOptaellingSettings settings)
        {
            return Gi.Batch.Shared.Crm.CrmConnectionStringFactory.Create(
                settings?.CrmConnectionTemplate,
                settings?.CrmServerName,
                settings?.CrmClientId,
                settings?.CrmClientSecret,
                settings?.CrmAuthority,
                settings?.CrmAuthorityMode);
        }

        public static string CreateSanitized(ContactRegistreringOptaellingSettings settings)
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
