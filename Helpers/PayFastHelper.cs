using System.Security.Cryptography;
using System.Text;

namespace Koolstoof_App_1.Helpers
{
    public static class PayFastHelper
    {
        // Builds the MD5 signature PayFast requires: concatenate every non-blank
        // field (in the order they were added) as urlencoded key=value pairs
        // joined with '&', append the passphrase if one is set, then MD5-hash it.
        // Field order matters — it must match the order the same fields are
        // rendered in the posted HTML form.
        public static string GenerateSignature(IEnumerable<KeyValuePair<string, string>> fields, string passphrase)
        {
            var sb = new StringBuilder();
            foreach (var field in fields)
            {
                if (string.IsNullOrEmpty(field.Value))
                {
                    continue;
                }
                if (sb.Length > 0)
                {
                    sb.Append('&');
                }
                sb.Append(field.Key).Append('=').Append(PhpUrlEncode(field.Value.Trim()));
            }

            if (!string.IsNullOrEmpty(passphrase))
            {
                sb.Append("&passphrase=").Append(PhpUrlEncode(passphrase.Trim()));
            }

            var hash = MD5.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        // PayFast's server recomputes the signature using PHP's urlencode(), which
        // differs from .NET's Uri.EscapeDataString (RFC 3986): PHP leaves only
        // A-Z a-z 0-9 - _ . unescaped and turns spaces into '+', while .NET's
        // encoder also leaves things like ~ ! * ' ( ) unescaped. Mismatched
        // encoding is the single most common cause of PayFast's "signature does
        // not match" error for non-PHP integrations, so this replicates PHP exactly.
        private static string PhpUrlEncode(string value)
        {
            var sb = new StringBuilder();
            foreach (var b in Encoding.UTF8.GetBytes(value))
            {
                var c = (char)b;
                if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c is '-' or '_' or '.')
                {
                    sb.Append(c);
                }
                else if (c == ' ')
                {
                    sb.Append('+');
                }
                else
                {
                    sb.Append('%').Append(b.ToString("X2"));
                }
            }
            return sb.ToString();
        }
    }
}
