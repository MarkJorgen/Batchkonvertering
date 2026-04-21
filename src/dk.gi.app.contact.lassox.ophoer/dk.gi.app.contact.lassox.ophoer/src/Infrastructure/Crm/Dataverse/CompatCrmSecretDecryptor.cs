namespace dk.gi.app.contact.lassox.ophoer.Infrastructure.Crm
{
    internal static class CompatCrmSecretDecryptor
    {
        public static string DecryptOrFallback(string value, out bool decrypted)
        {
            return Gi.Batch.Shared.Crm.CompatCrmSecretDecryptor.DecryptOrFallback(value, out decrypted);
        }
    }
}
