

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
                Console.WriteLine($"Author: {block.Author}");
                Console.WriteLine($"Data: {block.Data}");
                Console.WriteLine($"Hash: {block.Hash}");
                Console.WriteLine($"Nonce: {block.Nonce}");
                Console.WriteLine($"Difficulty: {block.Dificulty}");
                Console.WriteLine($"Previous Hash: {block.PreviousHash}");
                Console.WriteLine(new string('-', 40));
            }
        }

        public void PrintChainValidity(bool isValid)
        {
            Console.WriteLine(isValid ? "The blockchain is valid." : "The blockchain is invalid.");
        }
    }
}
