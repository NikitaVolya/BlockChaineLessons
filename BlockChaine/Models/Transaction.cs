
using System.Text;

namespace BlockChaine.Models
{
    public class Transaction : ICloneable
    {
        private const string MAIN_TOKEN_SYMBOL_CONSTANT = "MAIN";

        public static readonly string COINBASE_TOKEN = "COINBASE";
        public static readonly string MINTING_TOKEN = "MINT";

        public static readonly string MAIN_TOKEN_SYMBOL = MAIN_TOKEN_SYMBOL_CONSTANT;


        public String Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string TokenSymbol { get; set; } = MAIN_TOKEN_SYMBOL;
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal Fee { get; set; }

        public string Memo { get; set; }

        public byte[]? SenderPublicKey { get; set; } = null;
        public byte[]? Signature { get; set; } = null;

        public int LockTime { get; set; } = 0;

        private int _size;
        public int Size { get => _size; }

        public string? ReplaceTxId { get; set; } = null;

        public Transaction(string from, string to, decimal amount, string memo, decimal fee, string tokenSymbol = MAIN_TOKEN_SYMBOL_CONSTANT)
        {
            Id = Guid.NewGuid().ToString();
            From = from;
            To = to;
            TokenSymbol = tokenSymbol;
            Amount = amount;
            Memo = memo;
            TimeStamp = DateTime.UtcNow;
            Fee = fee;

            _size = GetDataToSign().Length;
        }

        public byte[] GetDataToSign()
        {
            var data = $"{From}|{To}|{TokenSymbol}|{Amount}|{Memo}|{TimeStamp:O}|{Fee}";
            return Encoding.UTF8.GetBytes(data);
        }

        public string ToRowString()
        {
            string signatureHex = Signature != null ? Convert.ToHexString(Signature) : String.Empty;
            return $"{Id}\t{From}\t{To}\t{TokenSymbol}\t{Amount}\t{Memo}\t{TimeStamp}\t{signatureHex}";
        }

        public override string ToString()
        {
            return $"Transaction ID: {Id} From: {From} To: {To} Amount: {Amount} {TokenSymbol} Fee: {Fee} Memo: {Memo} TimeStamp: {TimeStamp}";
        }

        public object Clone()
        {
            Transaction transaction = new Transaction(From, To, Amount, Memo, Fee, TokenSymbol)
            {
                Id = Id,
                TimeStamp = TimeStamp,
                SenderPublicKey = SenderPublicKey,
                Signature = Signature,
                LockTime = LockTime,
                ReplaceTxId = ReplaceTxId
            };

            transaction._size = _size;

            return transaction;
        }
    }
}
