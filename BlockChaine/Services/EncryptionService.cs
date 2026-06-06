using System.Security.Cryptography;


namespace BlockChaine.Services
{
    public class EncryptionService
    {
        public string Encrypt(string plainText, string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            using var keyDerivation = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);

            byte[] key = keyDerivation.GetBytes(32);

            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using MemoryStream ms = new();

            ms.Write(salt);
            ms.Write(aes.IV);

            using (CryptoStream cs = new(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            using (StreamWriter sw = new(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string encryptedText, string password)
        {
            byte[] data = Convert.FromBase64String(encryptedText);

            using MemoryStream ms = new(data);

            byte[] salt = new byte[16];
            ms.Read(salt, 0, salt.Length);

            byte[] iv = new byte[16];
            ms.Read(iv, 0, iv.Length);

            using var keyDerivation = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);

            byte[] key = keyDerivation.GetBytes(32);

            using Aes aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;

            using CryptoStream cs = new(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using StreamReader sr = new(cs);

            try
            {
                return sr.ReadToEnd();
            }
            catch (CryptographicException)
            {
                throw new Exception("Decryption failed. Incorrect password or corrupted data.");
            }
        }
    }
}
