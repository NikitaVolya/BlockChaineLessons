using BlockChaine.Models;
using System.Linq;

namespace BlockChaine.Services
{
    public class TransactionService
    {
        public Transaction CreateTransaction(string from, string to, decimal amount)
        {
            return new Transaction(from, to, amount);
        }

        public bool IsValid(Transaction transaction)
        {
            if (transaction == null)
                return false;
            if (string.IsNullOrWhiteSpace(transaction.From) || string.IsNullOrWhiteSpace(transaction.To))
                return false;
            if (transaction.Amount <= 0)
                return false;
            return true; 
        }

        public Transaction? FindLargestTransaction(List<Block> chain)
        {
            return chain.SelectMany(b => b.Transactions)
                        .OrderBy(t => -t.Amount)
                        .FirstOrDefault();
        }
    }
}
