using BlockChaine.Consensus;
using BlockChaine.Models;
using BlockChaine.Services;

namespace BlockChaine.Lessons
{
    internal class Lesson2
    {
        public static void RunClassWork()
        {
            var consensus = new ClearConsensusRule();

            var mainChain = new BlockChainService(consensus);
            var display = new BlockChainDisplayService();

            Console.WriteLine("=== MAIN CHAIN ===");

            mainChain.AddBlock("Alice", "-> Bob: 5 BTC");
            mainChain.AddBlock("Bob", "-> Charlie: 2 BTC");
            mainChain.AddBlock("Charlie", "-> Dave: 1 BTC");

            display.DisplayBlockChain(mainChain.Chain);

            Console.WriteLine("\nMAIN CHAIN VALID:");
            display.PrintChainValidity(mainChain.isChainValid());



            Console.WriteLine("\n=== ATTACKER MINING ===");

            var attackerBlocks = mainChain.Chain
                .Take(2)
                .Select(b => (Block)b.Clone())
                .ToList();

            var attackerChain = new BlockChainService(consensus);
            attackerChain.Chain = attackerBlocks;

            attackerChain.AddBlock("Hacker", "Attacker stole 1000 BTC");
            attackerChain.AddBlock("Alice", "Fake payment #1");
            attackerChain.AddBlock("Bob", "Fake payment #2");
            attackerChain.AddBlock("Alice", "Fake payment #3");

            Console.WriteLine("\nATTACKER CHAIN:");

            display.DisplayBlockChain(attackerChain.Chain);



            Console.WriteLine("\n=== MAIN CHAIN CONTINUES ===");

            mainChain.AddBlock("Alice", "Normal transaction");

            display.DisplayBlockChain(mainChain.Chain);


            Console.WriteLine("\n=== CONSENSUS RESOLUTION ===");

            mainChain.ResolveConsensus(attackerChain.Chain);

            Console.WriteLine("\n=== FINAL AUTHORITATIVE CHAIN ===");

            display.DisplayBlockChain(mainChain.Chain);

            Console.WriteLine("\nCHAIN VALID:");
            display.PrintChainValidity(mainChain.isChainValid());
        }



        public static void Run()
        {
            RunClassWork();
        }
    }
}
