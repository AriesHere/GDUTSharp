using System.Security.Cryptography;
using System.Text;
using GDUTSharp.Interfaces;
using GDUTSharp.Shared;
using Microsoft.Extensions.Logging;

namespace GDUTSharp.Services
{
    public partial class SecurityService(ILogger<SecurityService> logger) : ISecurityService
    {
        private readonly ILogger<SecurityService> _logger = logger;

        protected const int PREFIX_LENGTH = 64;

        /// <summary>基于 <see cref="AesChars"/> 生成随机字符</summary>
        protected virtual byte[] GenRandomString(int length)
        {
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++) bytes[i] = (byte)Random.Shared.Next(byte.MaxValue);
            return bytes;
        }

        public virtual byte[] GenIV() => GenRandomString(16);

        protected virtual byte[] GenPrefix() => GenRandomString(PREFIX_LENGTH);

        public virtual string CbcEncrypt(string plaintext, byte[] key, byte[] iv)
        {
            try
            {
                using Aes aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using ICryptoTransform encryptor = aes.CreateEncryptor();
                var plainBytes = (GenPrefix() + plaintext).ToBytes();
                byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Critical)) _logger.LogCritical("加密失败。{Exception}", e);
                throw;
            }
        }

        public virtual string CbcDecrypt(string cipherText, byte[] key, byte[] iv)
        {
            try
            {
                using Aes aes = Aes.Create();
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                using ICryptoTransform decryptor = aes.CreateDecryptor();
                byte[] cipherBytes = Convert.FromBase64String(cipherText);
                byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
                return plainBytes.GetString()[PREFIX_LENGTH..];
            }
            catch (Exception e)
            {
                if (_logger.IsEnabled(LogLevel.Critical)) _logger.LogCritical("解密失败。{Exception}", e);
                throw;
            }
        }
    }
}
