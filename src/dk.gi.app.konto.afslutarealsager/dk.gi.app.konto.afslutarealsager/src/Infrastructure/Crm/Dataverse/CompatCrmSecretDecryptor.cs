namespace dk.gi.app.konto.afslutarealsager.Infrastructure.Crm.Dataverse
{
    internal static class CompatCrmSecretDecryptor
    {
        public static string DecryptOrFallback(string value, out bool decrypted)
        {
            return Gi.Batch.Shared.Crm.CompatCrmSecretDecryptor.DecryptOrFallback(value, out decrypted);
        }
    }
}
