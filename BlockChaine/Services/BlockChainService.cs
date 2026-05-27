using BlockChaine.Consensus;
using BlockChaine.Models;

namespace BlockChaine.Services
{
    internal class BlockChainService
    {
        public List<Block> Chain { get; set; }

        public List<Transaction> PendingTransactions { get; set; }

        public decimal MiningReward { get; set; } = 50;


        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        private readonly TransactionService _transactionService;

        private readonly IConsesnsusRule _consensusRule;

        private readonly int _adjustmentInterval = 2;
        private readonly double _targetBlockTime = 10;

        private readonly int _maxPandingTransactionsParAddress = 5;

        public decimal BaseFeePerByte { get; set; } = 0.05m;
        public int MaxBlockSizeBytes {  get; set; } = 500;

        public int Dificulty => _consensusRule.GetDificulty();

        public BlockChainService(IConsesnsusRule consesnsusRule)
        {
            Chain = new List<Block>();

            PendingTransactions = new List<Transaction>();

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

        public decimal GetCurrentNetworkFee()
        {
            var MempoolSize = PendingTransactions.Sum(t => t.Size);
            if (MempoolSize < MaxBlockSizeBytes)
            {
                return BaseFeePerByte;
            }
            else
            {
                return BaseFeePerByte * (MempoolSize / MaxBlockSizeBytes);
            }
        }

        public void MinePendingTransactions(string minerAddress)
        {
            var previousBlock = Chain.Last();
            int blockSize = MaxBlockSizeBytes;

            var transactionsToInclude = new List<Transaction>();


            var reward = MiningReward;
            var rewardTransaction = new Transaction("COINBASE", minerAddress, reward, "reward for block", 0);

            transactionsToInclude.Add(rewardTransaction);
            blockSize -= rewardTransaction.Size;

            foreach (var transaction in PendingTransactions.OrderByDescending(t => t.Fee / t.Size))
            {
                if (transaction.Size > blockSize)
                    continue;

                transactionsToInclude.Add(transaction);
                blockSize -= transaction.Size;
            }

            rewardTransaction.Amount += PendingTransactions.Where(t => transactionsToInclude.Contains(t)).Sum(t => t.Fee);

            var newBlock = new Block(previousBlock.Index + 1, transactionsToInclude, previousBlock.Hash);

            newBlock.Dificulty = _consensusRule.GetDificulty();
            _miningService.MineBlock(newBlock);
            Chain.Add(newBlock);

            PendingTransactions = PendingTransactions.Where(t => !transactionsToInclude.Any(td => td.Id == t.Id)).ToList();

            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }
        }

        /*
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
        */

        public void AddTransaction(Transaction transaction)
        {
            if (!_transactionService.IsValid(transaction).isValid)
            {
                throw new Exception($"Invalid transaction: {transaction.Id}");
            }

            int fromWalletTransactionCount = PendingTransactions.Where(t => t.From == transaction.From).Count();
            if (fromWalletTransactionCount >= _maxPandingTransactionsParAddress)
            {
                throw new Exception($"Too many pending transactions from address: {transaction.From}. Max allowed is {_maxPandingTransactionsParAddress}.");
            }

            if (PendingTransactions.Exists(t => t.Id == transaction.Id) || Chain.Exists(b => b.Transactions.Exists(t => t.Id == transaction.Id)))
            {
                throw new Exception($"Transaction with id: {transaction.Id} is already exists in block chain or pending transactions\n");
            }

            if (transaction.ReplaceTxId != null)
            {
                Transaction? existingTransaction = PendingTransactions.FirstOrDefault(t => t.Id == transaction.ReplaceTxId);
                if (existingTransaction == null)
                {
                    throw new Exception($"No transaction found with id: {transaction.ReplaceTxId} to replace.");
                }
                if (existingTransaction.From != transaction.From)
                {
                    throw new Exception($"Transaction replacement must be from the same sender. Original transaction from: {existingTransaction.From}, replacement transaction from: {transaction.From}");
                }
                if (transaction.Fee <= existingTransaction.Fee)
                {
                    throw new Exception($"Replacement transaction fee must be higher than the original transaction fee. Original fee: {existingTransaction.Fee}, replacement fee: {transaction.Fee}");
                }
                PendingTransactions.Remove(existingTransaction);
            }

            decimal currentFeePerBlock = transaction.Size * GetCurrentNetworkFee();
            if (currentFeePerBlock > transaction.Fee)
            {
                throw new Exception($"Transaction fee is too low for transaction: {transaction.Id}. Minimum required fee is {currentFeePerBlock}");
            }

            if (transaction.From != "COINBASE" && GetPendingBalance(transaction.From) < transaction.Amount + transaction.Fee)
            {
                throw new Exception($"Insufficient balance for transaction: {transaction.Id}");
            }

            // todo Add check for sender balance
            PendingTransactions.Add(transaction);
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
            }
            else
            {
                _consensusRule.AddDificulty(-1);
            }
        }

        public void ClearChain()
        {
            Chain.Clear();
            CreateGenesisBlock();
        }

        public int GetCorruptedBlockIndex()
        {

            for (int i = 1; i < Chain.Count; i++)
            {

                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return i;
                if (currentBlock.PreviousHash != previousBlock.Hash)
                    return i;
            }

            return -1;
        }

        public void HackTheChain(int index)
        {

            for (int i = index; i < Chain.Count; i++)
            {

                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                currentBlock.PreviousHash = previousBlock.Hash;
                _miningService.MineBlock(currentBlock);
            }

        }

        public (bool isValid, string errorMessage) isValid()
        {
            return IsValidChain(Chain);
        }

        public (bool isValid, string errorMessage) IsValidChain(List<Block> chain)
        {
            const int timeErrorArea = 1; // Allow 1 minute of time discrepancy

            for (int i = 1; i < chain.Count; i++)
            {
                var currentBlock = chain[i];
                var previousBlock = chain[i - 1];

                if ((currentBlock.Timestamp - previousBlock.Timestamp).Microseconds <= 100)
                    return (false, $"Block {currentBlock.Index} has an invalid timestamp: {currentBlock.Timestamp}");
                if (DateTime.UtcNow.AddMinutes(timeErrorArea) < currentBlock.Timestamp)
                    return (false, $"Block {currentBlock.Index} has a timestamp from the future: {currentBlock.Timestamp}");
                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return (false, $"Block {currentBlock.Index} has an invalid hash: {currentBlock.Hash}");
                if (currentBlock.PreviousHash != previousBlock.Hash)
                    return (false, $"Block {currentBlock.Index} has an invalid previous hash: {currentBlock.PreviousHash}");

                if (currentBlock.Transactions != null)
                    foreach (var transaction in currentBlock.Transactions)
                    {
                        var transactionValidation = _transactionService.IsValid(transaction);
                        if (!transactionValidation.isValid)
                            return (false, $"Invalid transaction detected: {transaction.Id}\n{transactionValidation.errorMessage}");
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
                    .Select(block => (Block)block.Clone())
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


        private decimal GetBalance(string address)
        {
            decimal balance = 0;
            foreach (var block in Chain)
            {
                if (block.Transactions == null)
                    continue;
                
                foreach (var transaction in block.Transactions)
                {
                    if (transaction.From == address)
                    {
                        balance -= (transaction.Amount + transaction.Fee);
                    }
                    if (transaction.To == address)
                    {
                        balance += transaction.Amount;
                    }
                }
            }
            return balance;
        }

        public decimal GetPendingBalance(string address)
        {
            decimal balance = GetBalance(address);

            decimal pendingOutgoing = PendingTransactions
                .Where(t => t.From == address || t.To == address)
                .Sum(t => { 
                    decimal amount = 0;
                    if (t.From == address)
                        amount -= (t.Amount + t.Fee);
                    if (t.To == address)
                        amount += t.Amount;
                    return amount;
                });
            
            return balance + pendingOutgoing;
        }
    }
}
