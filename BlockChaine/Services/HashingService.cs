using BlockChaine.Models;
using System.Security.Cryptography;
using System.Text;

namespace BlockChaine.Services
{
    internal class HashingService
    {
        public string ComputeStringHashWitoutNonce(Block block)
        {
            MerkleTreeAudito merckleTreeAudit = new MerkleTreeAudito();
            
            List<List<string>> tree = merckleTreeAudit.BuildFullTree(block.Transactions);

            block.MerkleRoot = tree.Last().LastOrDefault() ?? "";
            block.MerkleTree = tree;

            return $"{block.Index}{block.Timestamp:O}{block.MerkleRoot}{block.PreviousHash}";
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

        public string CalculateMerkleRoot(List<Transaction> transactions)
        {
            if (transactions == null || transactions.Count == 0)
                return string.Empty;

            List<string> hashes = transactions.Select(t => ComputeSha256Hash(t.ToRowString())).ToList();
            while (hashes.Count > 1)
            {
                if (hashes.Count % 2 != 0)
                    hashes.Add(hashes.Last());

                List<string> newHashes = new List<string>();
                for (int i = 0; i < hashes.Count; i += 2)
                {
                    string combinedHash = ComputeSha256Hash(hashes[i] + hashes[i + 1]);
                    newHashes.Add(combinedHash);
                }

                hashes = newHashes;
            }
            return hashes[0];
        }
    }
}
