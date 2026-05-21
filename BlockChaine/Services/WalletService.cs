using BlockChaine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;


namespace BlockChaine.Services
{
    public class WalletService
    {
        private readonly int _addressLength = 16; // Спрощена довжина адреси

        public Wallet CreateWallet(string name)
        {

            using var ecdsa = System.Security.Cryptography.ECDsa.Create();
            var privateKey = ecdsa.ExportECPrivateKey();
            var publicKey = ecdsa.ExportSubjectPublicKeyInfo();
            var address = GenerateAddress(publicKey);
            return new Wallet(name, address, publicKey, privateKey);
        }

        public Wallet CreateVanityWallet(string name, string vanityPrefix)
        {
            int attempts = 0;

            object locker = new object();
            byte[] submitPrivateKey = null, submitPublicKey = null;
            string submitAddress = null;

            int taskCount = Environment.ProcessorCount;
            List<Task> tasks = new List<Task>();

            ECCurve curve = ECCurve.NamedCurves.nistP256; // Використовуємо криву P-256 для генерації ключів

            for (int i = 0; i < taskCount; i++)
            {
                var task = Task.Run(() =>
                {
                    using var ecdsa = System.Security.Cryptography.ECDsa.Create();
                    while (submitAddress == null)
                    {
                        var privateKey = ecdsa.ExportECPrivateKey();
                        var publicKey = ecdsa.ExportSubjectPublicKeyInfo();
                        var address = GenerateAddress(publicKey);

                        lock (locker)
                            attempts++;

                        if (address.StartsWith(vanityPrefix))
                        {
                            lock (locker)
                            {
                                if (submitAddress == null) // Перевіряємо ще раз, щоб уникнути перезапису
                                {
                                    submitAddress = address;
                                    submitPublicKey = publicKey;
                                    submitPrivateKey = privateKey;
                                }
                            }
                        }

                        ecdsa.GenerateKey(curve);
                    }
                });
                tasks.Add(task);
            }

            for (int i = 0; i < taskCount; i++)
                tasks[i].Wait();

            Console.WriteLine($"Vanity wallet found after {attempts} attempts: {submitAddress}");
            return new Wallet(name, submitAddress, submitPublicKey, submitPrivateKey);
        }

        public bool VerifySignature(byte[] data, byte[] signature, byte[] publicKey)
        {
            using var ecdsa = System.Security.Cryptography.ECDsa.Create();
            ecdsa.ImportSubjectPublicKeyInfo(publicKey, out _);
            return ecdsa.VerifyData(data, signature, System.Security.Cryptography.HashAlgorithmName.SHA256);
        }

        public string GenerateAddress(byte[] publicKey)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(publicKey));
        }

        public bool ValidateAddress(string address, byte[] publicKey)
        {
            var generatedAddress = GenerateAddress(publicKey);
            return generatedAddress == address;
        }
    }
}
