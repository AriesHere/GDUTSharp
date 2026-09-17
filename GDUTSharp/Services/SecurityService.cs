using System.Security.Cryptography;
using GDUTSharp.Interfaces;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services
{
    public partial class SecurityService(ILogger<SecurityService> logger) : ISecurityService
    {
        private readonly ILogger<SecurityService> _logger = logger;

        public virtual byte[] GenIV() => RandomNumberGenerator.GetBytes(16);

        public virtual byte[] CbcEncrypt(byte[] plaintext, byte[] key, byte[] iv)
        {
            try
            {
                using Aes aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using ICryptoTransform encryptor = aes.CreateEncryptor();
                return encryptor.TransformFinalBlock(plaintext, 0, plaintext.Length);
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Critical)) _logger.LogCritical("加密失败。{Exception}", e);
                throw;
            }
        }

        public virtual byte[] CbcDecrypt(byte[] cipherText, byte[] key, byte[] iv)
        {
            try
            {
                using Aes aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using ICryptoTransform decryptor = aes.CreateDecryptor();
                return decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Critical)) _logger.LogCritical("解密失败。{Exception}", e);
                throw;
            }
        }
    }
}
