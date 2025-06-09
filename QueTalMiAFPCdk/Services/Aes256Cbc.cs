using System;
using System.Security.Cryptography;
using System.Text;

namespace QueTalMiAFP.Services {
    public static class Aes256Cbc {
        public static string Encriptar(string plainText, string base64Key, string base64IV) {
            using (Aes aes = Aes.Create()) {
                aes.Key = Convert.FromBase64String(base64Key);
                aes.IV = Convert.FromBase64String(base64IV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateEncryptor()) {
                    Byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                    byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                    return Convert.ToBase64String(encryptedBytes);
                }
            }
        }

        public static string Desencriptar(string base64EncryptedText, string base64Key, string base64IV) {
            using (Aes aes = Aes.Create()) {
                aes.Key = Convert.FromBase64String(base64Key);
                aes.IV = Convert.FromBase64String(base64IV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (ICryptoTransform encryptor = aes.CreateDecryptor()) {
                    Byte[] encryptedTextBytes = Convert.FromBase64String(base64EncryptedText);
                    Byte[] plainTextBytes = encryptor.TransformFinalBlock(encryptedTextBytes, 0, encryptedTextBytes.Length);
                    return Encoding.UTF8.GetString(plainTextBytes);
                }
            }
        }
    }
}
