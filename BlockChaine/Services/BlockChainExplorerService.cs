
using BlockChaine.Models;

namespace BlockChaine.Services
{
    public class BlockChainExplorerService
    {
        private readonly BlockChainService _blockChainService;

        public BlockChainExplorerService(BlockChainService blockChainService)
        {
            _blockChainService = blockChainService;
        }

        public Transaction FindTransactionById(string txId)
        {
            Transaction? transaction;

            transaction = _blockChainService.Chain
                .SelectMany(block => block.Transactions)
                .FirstOrDefault(tx => tx.Id == txId);

            if (transaction == null)
            {
                transaction = _blockChainService.PendingTransactions
                    .FirstOrDefault(tx => tx.Id == txId);
            }

            if (transaction == null)
            {
                throw new Exception($"Transaction with ID {txId} not found.");
            }

            return transaction;
        }

        public Block FindBlockByTransactionId(string txId)
        {
            Block? block = _blockChainService.Chain.FirstOrDefault(b => b.Transactions.Any(tx => tx.Id == txId));

            if (block == null)
            {
                throw new Exception($"Block containing transaction ID {txId} not found.");
            }

            return block;
        }

        public List<Transaction> GetTransactionHistory(string address)
        {
            List<Transaction> transactions = _blockChainService.Chain
                .SelectMany(block => block.Transactions)
                .Where(t => t.From == address || t.To == address)
                .OrderByDescending(t => t.TimeStamp)
                .ToList();

            return transactions;
        }

        public decimal GetTotalFeesEarned(string minerAddress)
        {
            decimal totalFees = _blockChainService.Chain
                .Where(block => block.Transactions.Any(t => t.From == "COINBASE" && t.To == minerAddress))
                .SelectMany(block => block.Transactions)
                .Sum(t => t.Fee);

            return totalFees;
        }
    }
}
