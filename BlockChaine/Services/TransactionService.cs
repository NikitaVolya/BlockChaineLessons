using BlockChaine.Models;
using System.Linq;

namespace BlockChaine.Services
{
    public class TransactionService
    {
        private readonly WalletService _walletService;

        public TransactionService(WalletService walletService)
        {
            _walletService = walletService;
        }

        public Transaction CreateTransaction(Wallet sender, string to, decimal amount, string memo)
        {
            var tx = new Transaction(sender.Address, to, amount, memo);

            tx.SenderPublicKey = sender.PublicKey;
            tx.Signature = sender.Sign(tx.GetDataToSign());

            var validation = IsValid(tx);
            if (validation.Item1) {
                return tx;
            } else {
                throw new ArgumentException(validation.Item2);
            }
        }

        public (bool, string) IsValid(Transaction transaction)
        {
            if (transaction == null)
               return (false, "Transaction is null");

            if (string.IsNullOrWhiteSpace(transaction.From) || string.IsNullOrWhiteSpace(transaction.To))
                return (false, "Sender and recipient addresses must be provided");

            if (transaction.Amount <= 0)
                return (false, "Amount must be greater than zero");

            if (transaction.SenderPublicKey == null || transaction.Signature == null)
                return (false, "Transaction must include sender's public key and signature");

            if (!_walletService.ValidateAddress(transaction.From, transaction.SenderPublicKey))
                return (false, "Sender address does not match the provided public key");

            bool signatureValid = _walletService.VerifySignature(
                transaction.GetDataToSign(),
                transaction.Signature,
                transaction.SenderPublicKey
            );
            if (!signatureValid)
                return (false, "Invalid or broked transaction signature");

            return (true, "Transaction is valid");
        }

        public Transaction? FindLargestTransaction(List<Block> chain)
        {
            return chain.SelectMany(b => b.Transactions)
                        .OrderBy(t => -t.Amount)
                        .FirstOrDefault();
        }
    }
}
