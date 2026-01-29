using System;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace UserManagement.Infrastructure
{
    public class EnvironmentSetup
    {
        private readonly IConfiguration _configuration;
        public EnvironmentSetup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SetupEnvironmentVariables()
        {
            var encryptionKey = _configuration["Encryption:AESKey"];

            if (string.IsNullOrWhiteSpace(encryptionKey))
                throw new InvalidOperationException("Encryption key is missing in appsettings.json.");

            var encryptedPassword = FetchOrSetEnvironmentVariable("DATABASE_PASSWORD", _configuration["Encryption:DefaultEncryptedPassword"]);
            var encryptedServer = FetchOrSetEnvironmentVariable("DATABASE_SERVER", _configuration["Encryption:DefaultEncryptedServer"]);
            var encryptedUserId = FetchOrSetEnvironmentVariable("DATABASE_USERID", _configuration["Encryption:DefaultEncryptedUserId"]);

            var decryptedPassword = Decrypt(encryptedPassword, encryptionKey);
            var decryptedServer = Decrypt(encryptedServer, encryptionKey);
            var decryptedUserId = Decrypt(encryptedUserId, encryptionKey);

            Environment.SetEnvironmentVariable("DATABASE_PASSWORD", decryptedPassword, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("DATABASE_SERVER", decryptedServer, EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("DATABASE_USERID", decryptedUserId, EnvironmentVariableTarget.Process);
        }

        private static string Decrypt(string encryptedText, string key)
        {
            try
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] iv = new byte[16]; 

                using (Aes aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
                        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Information($"❌ Decryption failed: {ex.Message}");
                return "Decryption Failed!";
            }
        }

        private static string FetchOrSetEnvironmentVariable(string variableName, string defaultValue)
        {
            var value = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrWhiteSpace(value))
            {
                if (string.IsNullOrWhiteSpace(defaultValue))
                    throw new InvalidOperationException($"{variableName} is missing and no default value is provided.");

                Environment.SetEnvironmentVariable(variableName, defaultValue, EnvironmentVariableTarget.Process);
                value = defaultValue;
            }
            return value;
        }
    }
}
