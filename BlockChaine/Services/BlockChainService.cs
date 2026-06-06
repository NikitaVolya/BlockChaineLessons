using BlockChaine.Consensus;
using BlockChaine.Models;
using System.Numerics;

namespace BlockChaine.Services
{
    public class BlockChainService
    {
        public List<Block> Chain { get; set; }

        public List<Transaction> PendingTransactions { get; set; }

        public decimal MiningReward { get; set; } = 50;


        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        private readonly TransactionService _transactionService;
        private readonly FileService _fileService;

        private readonly IConsesnsusRule _consensusRule;

        private readonly int _adjustmentInterval = 2;
        private readonly double _targetBlockTime = 10;


        private readonly int _maxPandingTransactionsParAddress = 5;

        private readonly int _blockMaxFuturHoursTrashHold = 2;

        public decimal BaseFeePerByte { get; set; } = 0.05m;

        public int MaxTransactionPerBlock { get; set; } = 10;
        public int MaxBlockSizeBytes { get; set; } = 500;
        public const int MaxReorgDepth = 5;


        public int CoinbaseMaturity { get; set; } = 3;

        public int Dificulty => _consensusRule.GetDificulty();

        public BlockChainService(IConsesnsusRule consesnsusRule, FileService fileService)
        {
            PendingTransactions = new List<Transaction>();

            _hashingService = new HashingService();
            _miningService = new MiningService(consesnsusRule);
            _transactionService = new TransactionService(new WalletService());
            _fileService = fileService;

            _consensusRule = consesnsusRule;

            var loadedChain = _fileService.LoadChain();
            if (loadedChain.Any())
            {
                Chain = loadedChain;
            }
            else
            {
                Chain = new List<Block>();
                CreateGenesisBlock();
            }

        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, new List<Transaction>(), "0");
            genesisBlock.Timestamp = new DateTime(2024, 1, 1);

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

        public Block MinePendingTransactions(string minerAddress)
        {
            var previousBlock = Chain.Last();
            int blockSize = MaxBlockSizeBytes;

            var transactionsToInclude = new List<Transaction>();


            var reward = MiningReward;
            var rewardTransaction = new Transaction("COINBASE", minerAddress, reward, "reward for block", 0);

            transactionsToInclude.Add(rewardTransaction);
            blockSize -= rewardTransaction.Size;

            foreach (var transaction in PendingTransactions.Where(t => t.LockTime <= Chain.Last().Index).OrderByDescending(t => t.Fee / t.Size))
            {
                if (transactionsToInclude.Count >= MaxTransactionPerBlock)
                    break;
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

            _fileService.SaveChain(Chain);

            PendingTransactions = PendingTransactions.Where(t => !transactionsToInclude.Any(td => td.Id == t.Id)).ToList();

            if (newBlock.Index % _adjustmentInterval == 0)
            {
                AdjustDifficulty();
            }

            return newBlock;
        }

        public void AddTransaction(Transaction transaction, int lockTime = 0)
        {
            // lock time validation: lock time cannot be negative and cannot be too far in the future
            if (lockTime < 0)
            {
                throw new Exception("Lock time cannot be negative.");
            }

            // Transaction validation
            if (!_transactionService.IsValid(transaction).isValid)
            {
                throw new Exception($"Invalid transaction: {transaction.Id}");
            }

            // Check for too many pending transactions from the same address
            int fromWalletTransactionCount = PendingTransactions.Where(t => t.From == transaction.From).Count();
            if (fromWalletTransactionCount >= _maxPandingTransactionsParAddress)
            {
                throw new Exception($"Too many pending transactions from address: {transaction.From}. Max allowed is {_maxPandingTransactionsParAddress}.");
            }

            // Check for duplicate transaction in pending transactions and blockchain
            if (PendingTransactions.Exists(t => t.Id == transaction.Id) || Chain.Exists(b => b.Transactions.Exists(t => t.Id == transaction.Id)))
            {
                throw new Exception($"Transaction with id: {transaction.Id} is already exists in block chain or pending transactions\n");
            }

            // If this transaction is a replacement for an existing transaction, validate the replacement rules
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

            // Check if the transaction fee is sufficient based on the current network fee and transaction size
            decimal currentFeePerBlock = transaction.Size * GetCurrentNetworkFee();
            if (currentFeePerBlock > transaction.Fee)
            {
                throw new Exception($"Transaction fee is too low for transaction: {transaction.Id}. Minimum required fee is {currentFeePerBlock}");
            }

            // Check if the sender has sufficient balance for the transaction amount and fee (only for non-coinbase and non-minting transactions)
            if (transaction.From != Transaction.COINBASE_TOKEN &&
                transaction.From != Transaction.MINTING_TOKEN)
            {
                // Check if the sender has sufficient balance for the transaction fee
                if (GetPendingBalance(transaction.From, Transaction.MAIN_TOKEN_SYMBOL) < transaction.Fee)
                {
                    throw new Exception($"Insufficient balance to cover transaction fee for transaction: {transaction.Id}");
                }

                // Check if the sender has sufficient balance for the transaction amount
                decimal minimumRequiredBalance = transaction.Amount;

                if (transaction.TokenSymbol == Transaction.MAIN_TOKEN_SYMBOL)
                    minimumRequiredBalance += transaction.Fee;

                if (GetPendingBalance(transaction.From, transaction.TokenSymbol) < minimumRequiredBalance)
                {
                    throw new Exception($"Insufficient balance for transaction: {transaction.Id}");
                }
            }

            transaction.LockTime = Chain.Count + lockTime;

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

        public (bool result, string errorMessage) isValid()
        {
            (bool isValide, string errorMessage) = IsValidChain(Chain);
            return (isValide, errorMessage);
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


        private decimal GetBalance(string address, string tokenSymbol)
        {
            decimal balance = 0;
            foreach (var block in Chain)
            {
                if (block.Transactions == null)
                    continue;

                foreach (var transaction in block.Transactions)
                {
                    // Skip transactions that are not related to the specified token symbol
                    if (transaction.TokenSymbol != tokenSymbol)
                        continue;

                    if (transaction.From == address)
                    {
                        balance -= transaction.Amount;

                        if (transaction.TokenSymbol == Transaction.MAIN_TOKEN_SYMBOL)
                            balance -= transaction.Fee;
                    }
                    if (transaction.To == address)
                    {
                        /// Prevents counting unspendable coinbase transactions in balance calculation
                        /// and also prevents counting coinbase transactions that are not yet matured
                        if (transaction.From == "COINBASE" && CoinbaseMaturity > Chain.Count - block.Index)
                            continue;
                        
                        balance += transaction.Amount;
                    }
                }
            }
            return balance;
        }

        public decimal GetPendingBalance(string address, string tokenSymbol)
        {
            decimal balance = GetBalance(address, tokenSymbol);

            decimal pendingOutgoing = PendingTransactions
                .Where(t => (t.From == address || t.To == address) && t.TokenSymbol == tokenSymbol)
                .Sum(t =>
                {
                    decimal amount = 0;
                    if (t.From == address)
                    {
                        amount -= t.Amount;
                        if (t.TokenSymbol == Transaction.MAIN_TOKEN_SYMBOL)
                            amount -= t.Fee;
                    }
                    if (t.To == address)
                        amount += t.Amount;
                    return amount;
                });

            return balance + pendingOutgoing;
        }

        public int GetTransactionConfirmations(string transactionId)
        {
            int confirmations = -1;

            foreach (var block in Chain)
            {
                if (block.Transactions == null)
                    continue;
                if (block.Transactions.Any(t => t.Id == transactionId))
                {
                    confirmations = Chain.Count - block.Index;
                    break;
                }
            }

            return confirmations;
        }

        public bool TryAddBlockFromPeer(Block block)
        {
            var lastBlock = Chain.Last();
            if (block.PreviousHash != lastBlock.Hash)
            {
                Console.WriteLine($"Received block with invalid previous hash: {block.PreviousHash}");
                return false;
            }

            // Validate block hash
            if (block.Hash != _hashingService.ComputeHash(block))
            {
                Console.WriteLine($"Received block with invalid hash: {block.Hash}");
                return false;
            }

            // Time validation: block timestamp should be greater than previous block and not from the future
            if (block.Timestamp <= lastBlock.Timestamp || block.Timestamp > DateTime.UtcNow.AddHours(_blockMaxFuturHoursTrashHold))
            {
                Console.WriteLine($"Received block with invalid timestamp: {block.Timestamp}");
                return false;
            }

            if (!_consensusRule.IsValid(block.Hash, block.Dificulty))
            {
                Console.WriteLine($"Received block does not meet consensus rules. Hash: {block.Hash}, Dificulty: {block.Dificulty}");
                return false;
            }

            // Validate transactions in the block
            foreach (var transaction in block.Transactions)
            {
                if (!_transactionService.IsValid(transaction).isValid){
                    return false;
                }
            }

            Chain.Add(block);
            foreach (var transaction in block.Transactions)
            {
                PendingTransactions.RemoveAll(t => t.Id == transaction.Id);
            }

            _fileService.SaveChain(Chain);
            return true;
        }

        public bool ResolveConflicts(List<Block> peerChain)
        {

            if (!IsValidChain(peerChain).isValid)
            {
                Console.WriteLine("Peer chain is invalid. Cannot resolve conflicts.");
                return false;
            }

            if (peerChain.Count <= Chain.Count)
            {
                Console.WriteLine("Peer chain is not longer than current chain. No need to resolve conflicts.");
                return false;
            }

            int forkIndex = -1;
            for (int i = 0; i < Math.Min(Chain.Count, peerChain.Count); i++)
            {
                if (Chain[i].Hash != peerChain[i].Hash)
                {
                    forkIndex = i;
                    break;
                }
            }

            if (forkIndex == -1)
            {
                Console.WriteLine("No fork detected. Chains are identical.");
                return false;
            }

            if (Chain.Count - forkIndex > MaxReorgDepth)
            {
                Console.WriteLine($"Fork detected at index {forkIndex}, but it's too deep to reorganize (max reorg depth is {MaxReorgDepth}).");
                return false;
            }

            int currentChainPOW = Chain.Sum(b => b.Dificulty);
            int peerChainPOW = peerChain.Sum(b => b.Dificulty);

            if (peerChainPOW < currentChainPOW)
            {
                Console.WriteLine($"Peer chain has less cumulative proof of work ({peerChainPOW}) than current chain ({currentChainPOW}). No need to resolve conflicts.");
                return false;
            }

            Console.WriteLine("Peer chain is longer and valid. Replacing current chain.");
            Chain = peerChain.Select(block => (Block)block.Clone()).ToList();

            _fileService.SaveChain(Chain);

            return true;
        }
    }
}
