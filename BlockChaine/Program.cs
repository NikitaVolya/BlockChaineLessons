

using BlockChaine.Consensus;
using BlockChaine.Lessons;
using BlockChaine.Services;


var display = new BlockChainDisplayService();
var watch = new System.Diagnostics.Stopwatch();
var blockchain = new BlockChainService(new POWConsesnsusRule(0));



for (int i = 0; i < 30; i++)
{
    var difficulty = blockchain.Dificulty;

    watch.Reset();
    watch.Start();
    blockchain.AddBlock("Alice", "-> Bob: 5 BTC");
    watch.Stop();

    Console.WriteLine($"Block {blockchain.Chain.Last().Index} added with hash: {blockchain.Chain.Last().Hash} and dificulty: {blockchain.Chain.Last().Dificulty}");
    Console.WriteLine($" time taken for block with dificulty {difficulty}: {watch.ElapsedMilliseconds} ms");

    blockchain.PrintDificultyHistory();
}

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();