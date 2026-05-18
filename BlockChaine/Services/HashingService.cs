using BlockChaine.Models;
using System.Security.Cryptography;
using System.Text;

namespace BlockChaine.Services
{
    internal class HashingService
    {
        public string ComputeStringHashWitoutNonce(Block block)
        {
            return $"{block.Index}{block.Timestamp:O}{block.Author}{block.Data}{block.PreviousHash}";
        }

        public string ComputeHash(Block block) {

            byte[] staticBytes = Encoding.UTF8.GetBytes(ComputeStringHashWitoutNonce(block));
            byte[] buffer = new byte[staticBytes.Length + sizeof(int)];
            Buffer.BlockCopy(staticBytes, 0, buffer, 0, staticBytes.Length);

            BitConverter.TryWriteBytes(
                buffer.AsSpan(staticBytes.Length),
                block.Nonce
            );

            string res = String.Empty;
            using (SHA256 sha256 = SHA256.Create())
                res = Convert.ToHexString(sha256.ComputeHash(buffer)).ToLower();
            return res;
        }

        public string ComputeSha256Hash(string input)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(bytes);
                return Convert.ToHexString(hashBytes).ToLower();
            }
        }
    }
}
