using BlockChaine.Models;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;


namespace BlockChaine.Services
{
    public class WalletKeystoreService
    {
        public static readonly string DefaultFileName = "wallet.dat";

        private readonly EncryptionService _encryptionService;

        public WalletKeystoreService()
        {
            _encryptionService = new EncryptionService();
        }


        public bool SaveWallet(Wallet wallet, string password, string filePath)
        {
            bool res = true;

            string jsonData  = JsonSerializer.Serialize(wallet);
            string encryptedData = _encryptionService.Encrypt(jsonData, password);

            try
            {
                File.WriteAllText(filePath, encryptedData);
            } catch (Exception ex)
            {
                Debug.WriteLine($"Error saving wallet to file: {ex.Message}");
                res = false;
            }

            return res;
        }

        public bool SaveWallet(Wallet wallet, string password)
        {
            return SaveWallet(wallet, password, DefaultFileName);
        }

        public Wallet LoadWalletFromFile(string password, string filePath)
        {

            string encryptedData = File.ReadAllText(filePath);

            Wallet wallet;
            try
            {
                string jsonData = _encryptionService.Decrypt(encryptedData, password);

                wallet = JsonSerializer.Deserialize<Wallet>(jsonData)!;

            } catch (Exception ex) {
                throw new Exception("Failed to load wallet from file. Invalid data or incorrect password.");
            }

            return wallet;
        }
    }
}
