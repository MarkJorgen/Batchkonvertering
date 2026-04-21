using System.Collections.Generic;

namespace Gi.Batch.Shared.Configuration
{
    public interface IAzureSettingsSource
    {
        IReadOnlyDictionary<string, string> Load(string connectionString);
    }
}
