using BlockChaine.Consensus;
using BlockChaine.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlockChaine.Services
{
    internal class BlockChainService
    {
        public List<Block> Chain { get; set; }

        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        private readonly TransactionService _transactionService;

        private readonly IConsesnsusRule _consensusRule;

        private readonly int _adjustmentInterval = 2;
        private readonly double _targetBlockTime = 10;

        private readonly int _transactionLimitPerBlock = 3;

        public int Dificulty => _consensusRule.GetDificulty();

        public BlockChainService(IConsesnsusRule consesnsusRule)
        {
            Chain = new List<Block>();

            _hashingService = new HashingService();
            _miningService = new MiningService(consesnsusRule);
            _transactionService = new TransactionService(new WalletService());

            _consensusRule = consesnsusRule;
            
            CreateGenesisBlock();
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, new List<Transaction>(), "0");
            _miningService.MineBlock(genesisBlock);
            Chain.Add(genesisBlock);
        }

        public void AddBlock(List<Transaction> data)
        {
            if (data.Count > _transactionLimitPerBlock)
                throw new Exception($"Transaction limit per block exceeded. Max allowed is {_transactionLimitPerBlock}.");

            if (data.Count == 0 && Chain.Count != 0)
                throw new Exception("Cannot add an empty block. Please provide at least one transaction.");

            foreach (var transaction in data)
            {
                var transactionValidation = _transactionService.IsValid(transaction);
                if (transaction.Signature == null || !transactionValidation.Item1)
                    throw new Exception($"Invalid transaction detected: {transaction.Id}\n{transactionValidation.Item2}");
                if (Chain.Exists(b => b.Transactions.Exists(t => t.Id == transaction.Id)))
                    throw new Exception($"Transaction with id: {transaction.Id} is already exists in block chain\n");
            }

            var previousBlock = Chain.Last();
            var newBlock = new Block(previousBlock.Index + 1, data, previousBlock.Hash);

            _miningService.MineBlock(newBlock);
            newBlock.Dificulty = _consensusRule.GetDificulty();
            Chain.Add(newBlock);
            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }
        }

        private void AdjustDifficulty()
        {
            var recentBlocks = Chain.Where(b => b.Index != 0).TakeLast(_adjustmentInterval).ToList();

            if (recentBlocks.Count == 0)
                return;

            double averageTime = recentBlocks.Average(b => (b.Timestamp - Chain[b.Index - 1].Timestamp).TotalSeconds);
            if (averageTime < _targetBlockTime)
            {
                _consensusRule.AddDificulty(1);
            } else
            {
                _consensusRule.AddDificulty(-1);
            }
        }

        public void ClearChain()
        {
            Chain.Clear();
            CreateGenesisBlock();
        }

        public int GetCorruptedBlockIndex() {

            for (int i = 1; i < Chain.Count; i++) { 
                
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return i;
                if (currentBlock.PreviousHash != previousBlock.Hash)
                    return i;
            }

            return -1;
        }

        public void HackTheChain(int index) {

            for (int i = index; i < Chain.Count; i++) { 
                
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                currentBlock.PreviousHash = previousBlock.Hash;
                _miningService.MineBlock(currentBlock);
            }

        }

        public (bool, string) isValid()
        {
            return IsValidChain(Chain);
        }

        public (bool, string) IsValidChain(List<Block> chain)
        {
            const int timeErrorArea = 1; // Allow 1 minute of time discrepancy

            for (int i = 1; i < chain.Count; i++)
            {
                var currentBlock = chain[i];
                var previousBlock = chain[i - 1];

                if (currentBlock.Transactions == null)
                    return (false, "");
                if ((currentBlock.Timestamp - previousBlock.Timestamp).Microseconds <= 100)
                    return (false, $"Block {currentBlock.Index} has an invalid timestamp: {currentBlock.Timestamp}");
                if (DateTime.UtcNow.AddMinutes(timeErrorArea) < currentBlock.Timestamp)
                    return (false, $"Block {currentBlock.Index} has a timestamp from the future: {currentBlock.Timestamp}");
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return (false, $"Block {currentBlock.Index} has an invalid hash: {currentBlock.Hash}");
                if (currentBlock.PreviousHash != previousBlock.Hash)
                    return (false, $"Block {currentBlock.Index} has an invalid previous hash: {currentBlock.PreviousHash}");

                foreach (var transaction in currentBlock.Transactions)
                {
                    var transactionValidation = _transactionService.IsValid(transaction);
                    if (transaction.Signature == null || !transactionValidation.Item1)
                        return (false, $"Invalid transaction detected: {transaction.Id}\n{transactionValidation.Item2}");
                }

            }

            return (true, "Chain is valid.");
        }

        public bool ResolveConsensus(List<Block> competingChain)
        {
            Console.WriteLine("Resolving consensus...");

            if (!IsValidChain(competingChain).Item1)
            {
                Console.WriteLine("Competing chain is invalid.");
                return false;
            }

            if (competingChain.Count > Chain.Count)
            {
                Console.WriteLine("Competing chain wins. Reorganizing blockchain...");

                Chain = competingChain
                    .Select(block => (Block) block.Clone())
                    .ToList();

                return true;
            }

            Console.WriteLine("Current chain remains authoritative.");
            return false;
        }

        public void PrintDificultyHistory()
        {
            Console.Write("Dificulty history: ");
            foreach (var block in Chain)
            {
                Console.Write($"{block.Dificulty} -> ");
            }
            Console.WriteLine();
        }
    }
}
