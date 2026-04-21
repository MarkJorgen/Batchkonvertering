using System;
using System.Collections.Generic;
using System.Configuration;

namespace Gi.Batch.Shared.Configuration
{
    public sealed class JobConfiguration
    {
        private readonly Dictionary<string, string> _values;

        public JobConfiguration(IDictionary<string, string> values)
        {
            _values = new Dictionary<string, string>(
                values ?? new Dictionary<string, string>(),
                StringComparer.OrdinalIgnoreCase);
        }

        public IReadOnlyDictionary<string, string> Values => _values;

        public string this[string key] => Get(key);

        public string Get(string key, string defaultValue = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("Configuration key must be provided.", nameof(key));
            }

            string value;
            return _values.TryGetValue(key, out value) ? value : defaultValue;
        }

        public bool TryGet(string key, out string value)
        {
            return _values.TryGetValue(key, out value);
        }

        public string GetRequired(string key)
        {
            string value = Get(key);

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ConfigurationErrorsException("Required setting is missing: " + key);
            }

            return value;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            string value = Get(key);

            bool parsed;
            return bool.TryParse(value, out parsed) ? parsed : defaultValue;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            string value = Get(key);

            int parsed;
            return int.TryParse(value, out parsed) ? parsed : defaultValue;
        }
    }
}
