using System.Security.Cryptography;
using System.Text;

namespace PaintBooth.database.Helper
{
    public static class EncryptionHelper
    {
        // ── MD5 Hash ──────────────────────────────────────────────
        // Same style as Encryptdata() in your existing code.
        // Whatever method you currently use — just replace the body here.
        public static string Encrypt(string plainText)
        {
            using var md5  = MD5.Create();
            var inputBytes = Encoding.UTF8.GetBytes(plainText);
            var hashBytes  = md5.ComputeHash(inputBytes);

            return Convert.ToHexString(hashBytes).ToLower();
        }
    }
}
