using System;
using System.Text;

namespace Gi.Batch.Shared.Crm
{
    public static class CompatCrmSecretDecryptor
    {
        private const int EncryptionKey = 17;

        public static string DecryptOrFallback(string value, out bool decrypted)
        {
            decrypted = false;
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            try
            {
                byte[] bytes = Convert.FromBase64String(value);
                string input = ToStringNoEncoding(bytes);
                var output = new StringBuilder(input.Length);
                for (int i = 0; i < input.Length; i++)
                {
                    output.Append((char)(input[i] ^ EncryptionKey));
                }

                decrypted = true;
                return output.ToString();
            }
            catch
            {
                return value;
            }
        }

        private static string ToStringNoEncoding(byte[] bytes)
        {
            char[] chars = new char[bytes.Length / sizeof(char)];
            Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
            return new string(chars);
        }
    }
}
