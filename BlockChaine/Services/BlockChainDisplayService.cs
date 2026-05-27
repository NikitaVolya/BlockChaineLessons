

using BlockChaine.Models;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace BlockChaine.Services
{
    internal class BlockChainDisplayService
    {

        public void DisplayBlockChain(List<Models.Block> chain)
        {
            foreach (var block in chain)
            {
                Console.WriteLine($"Index: {block.Index}");
                Console.WriteLine($"Timestamp: {block.Timestamp}");
                Console.WriteLine($"Transactions:");
                Console.WriteLine(new string('-', 30));
                foreach (var transaction in block.Transactions)
                {
                    Console.WriteLine($"{transaction}");
                    Console.WriteLine(new string('-', 30));
                }
                Console.WriteLine($"Hash: {block.Hash}");
                Console.WriteLine($"Nonce: {block.Nonce}");
                Console.WriteLine($"Difficulty: {block.Dificulty}");
                Console.WriteLine($"Previous Hash: {block.PreviousHash}");
                Console.WriteLine(new string('-', 40));
                Console.WriteLine();
            }
        }

        public void PrintTransactionHistory(List<Models.Block> chain, string address)
        {
            Console.WriteLine($"Transaction history for address: {address}");
            Console.WriteLine(new string('=', 40));

            address = address.ToLower();
            bool hasTransactions = false;

            foreach (var block in chain.Skip(1))
            {
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.From.ToLower() == address || transaction.To.ToLower() == address)
                    {
                        Console.WriteLine($"Block Index: {block.Index}");
                        Console.WriteLine($"{transaction}");
                        Console.WriteLine(new string('-', 30));
                        hasTransactions = true;
                    }
                }
            }

            if (!hasTransactions)
            {
                Console.WriteLine("No transactions found for this address.");
            }
        }

        public void PrintPendingTransactions(List<Transaction> pendingTransactions)
        {
            Console.WriteLine("Pending Transactions:");
            Console.WriteLine(new string('=', 40));
            if (pendingTransactions.Count == 0)
            {
                Console.WriteLine("No pending transactions.");
                return;
            }
            foreach (var transaction in pendingTransactions)
            {
                Console.WriteLine($"{transaction}");
                Console.WriteLine(new string('-', 30));
            }
        }

        public void PrintWallet(Wallet wallet)
        {
            string shortPublicKey = Convert.ToHexString(wallet.PublicKey);

            if (shortPublicKey.Length > 20)
                shortPublicKey = shortPublicKey[..20] + "...";

            string[] lines =
            {
                $"   Owner     : {wallet.Name}",
                $"   Address   : {wallet.Address}",
                $"   PublicKey : {shortPublicKey}"
            };

            int width = lines.Max(l => l.Length);

            Console.WriteLine($"╔{new string('═', width + 2)}╗");

            foreach (var line in lines)
            {
                Console.WriteLine($"║ {line.PadRight(width)} ║");
            }

            Console.WriteLine($"╚{new string('═', width + 2)}╝");
        }
        public void PrintChainValidity(bool isValid)
        {
            Console.WriteLine(isValid ? "The blockchain is valid." : "The blockchain is invalid.");
        }
    }
}
