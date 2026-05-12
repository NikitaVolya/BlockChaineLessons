using BlockChaine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChaine.Services
{
    public class HashingService
    {
        public string ComputeHash(Block block) {
            var input = $"{block.Index}{block.Timestamp:O}{block.Author}{block.Data}{block.PreviousHash}{block.Nonce}";
            return ComputeSha256Hash(input);
        }

        private string ComputeSha256Hash(string input)
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
