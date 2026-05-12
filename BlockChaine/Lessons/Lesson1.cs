

using BlockChaine.Consensus;
using BlockChaine.Services;

namespace BlockChaine.Lessons
{
    public class Lesson1
    {
        public static void RunHomeWork()
        {
            var consensus = new ClearConsensusRule();

            var mainChain = new BlockChainService(consensus);
            var display = new BlockChainDisplayService();

            Console.WriteLine("=== MAIN CHAIN ===");

            mainChain.AddBlock("Alice", "-> Bob: 5 BTC");
            mainChain.AddBlock("Bob", "-> Charlie: 2 BTC");
            mainChain.AddBlock("Charlie", "-> Dave: 1 BTC");

            display.DisplayBlockChain(mainChain.Chain);
            display.PrintChainValidity(mainChain.isValid());


            Console.WriteLine("\n\n=== AFTER CHANGING ===\n");

            mainChain.Chain[1].Data = "-> Bob: 500 BTC";

            display.DisplayBlockChain(mainChain.Chain);
            display.PrintChainValidity(mainChain.isValid());
        }

        public static void Run()
        {
            RunHomeWork();
        }
    }
}
