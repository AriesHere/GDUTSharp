using System.Security.Cryptography;
using GDUTSharp.Interfaces;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services
{
    public partial class SecurityService(ILogger<SecurityService> logger) : ISecurityService
    {
        private readonly ILogger<SecurityService> _logger = logger;

        private const string INIT_VECTOR = "Jisniwqjwqjwqjww";
        private const string PREFIX = "J69IVxcXqvqNhvk1J69IVxcXqvqNhvk1J69IVxcXqvqNhvk1J69IVxcXqvqNhvk1";

        public string CbcEncrypt(string plaintext, string key)
        {
            string iv = INIT_VECTOR;
            string s = PREFIX + plaintext;
            try
            {
                return BaseEncrypt(s, key, iv);
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Critical)) _logger.LogCritical("加密失败。{Exception}", e);
                throw new Exception("CBC encryption failed", e);
            }
        }

        public string CbcDecrypt(string cipherText, string key)
        {
            string iv = INIT_VECTOR;
            try
            {
                return BaseDecrypt(cipherText, key, iv).Substring(PREFIX.Length);
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Critical)) _logger.LogCritical("解密失败。{Exception}", e);
                throw new Exception("CBC decryption failed", e);
            }
        }

        private static string BaseEncrypt(string plainText, string key, string iv)
        {
            byte[] encrypted = AesCbcEncrypt(plainText.GetBytes(), key.GetBytes(), iv.GetBytes());
            return Convert.ToBase64String(encrypted);
        }

        private static string BaseDecrypt(string cipherBase64, string key, string iv)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherBase64);
            byte[] plainBytes = AesCbcDecrypt(cipherBytes, key.GetBytes(), iv.GetBytes());
            return plainBytes.GetString();
        }

        private static byte[] AesCbcEncrypt(byte[] plaintext, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using ICryptoTransform encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
        }

        private static byte[] AesCbcDecrypt(byte[] ciphertext, byte[] key, byte[] iv)
        {
            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;
            using ICryptoTransform decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
        }
    }
}
