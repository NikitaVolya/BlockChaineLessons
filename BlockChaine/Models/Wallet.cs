

namespace BlockChaine.Models
{
    public class Wallet
    {
        public string Name { get; } // Не обовзязкове, для зручності
        public string Address { get; }
        public byte[] PublicKey { get; }
        private byte[] PrivateKey { get; }

        public Wallet(string name, string address, byte[] publicKey, byte[] privateKey)
        {
            Name = name;
            Address = address;
            PublicKey = publicKey;
            PrivateKey = privateKey;
        }

        public byte[] Sign(byte[] data)
        {
            using var ecdsa = System.Security.Cryptography.ECDsa.Create();
            
            ecdsa.ImportECPrivateKey(PrivateKey, out _);
            return ecdsa.SignData(data, System.Security.Cryptography.HashAlgorithmName.SHA256);
        }
    }
}
