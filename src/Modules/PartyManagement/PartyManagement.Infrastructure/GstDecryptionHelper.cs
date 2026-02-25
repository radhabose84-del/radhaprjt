using System.Security.Cryptography;
using System.Text;

namespace PartyManagement.Infrastructure
{
    public static class GstDecryptionHelper
    {
        public static string DecryptSek(string encryptedSek, string appKeyBase64)
        {
            byte[] appKey = Convert.FromBase64String(appKeyBase64);
            byte[] sekBytes = Convert.FromBase64String(encryptedSek);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Key = appKey;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            byte[] plainSek = decryptor.TransformFinalBlock(sekBytes, 0, sekBytes.Length);

            return Convert.ToBase64String(plainSek); // ✅ returns valid 32-byte key
        }

        public static string DecryptData(string encryptedData, string decryptedSekBase64)
        {
            byte[] sekKey = Convert.FromBase64String(decryptedSekBase64);
            byte[] dataBytes = Convert.FromBase64String(encryptedData);

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.Key = sekKey;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;

            using var decryptor = aes.CreateDecryptor();
            byte[] decryptedBytes = decryptor.TransformFinalBlock(dataBytes, 0, dataBytes.Length);

            return Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');
        }     
        
    }
}
