using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChaine.Models
{
    public class Transaction : ICloneable
    {
        public String Id { get; set; }
        public string From { get; set; }
        public string To { get; set; }
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

        public Transaction(string from, string to, decimal amount, string memo, decimal fee)
        {
            Id = Guid.NewGuid().ToString();
            From = from;
            To = to;
            Amount = amount;
            Memo = memo;
            TimeStamp = DateTime.UtcNow;
            Fee = fee;

            _size = GetDataToSign().Length;
        }

        public byte[] GetDataToSign()
        {
            var data = $"{From}|{To}|{Amount}|{Memo}|{TimeStamp:O}|{Fee}";
            return Encoding.UTF8.GetBytes(data);
        }

        public string ToRowString()
        {
            string signatureHex = Signature != null ? Convert.ToHexString(Signature) : String.Empty;
            return $"{Id}\t{From}\t{To}\t{Amount}\t{Memo}\t{TimeStamp}\t{signatureHex}";
        }

        public override string ToString()
        {
            return $"Transaction ID: {Id} From: {From} To: {To} Amount: {Amount} Fee: {Fee} Memo: {Memo} TimeStamp: {TimeStamp}";
        }

        public object Clone()
        {
            return new Transaction(From, To, Amount, Memo, Fee)
            {
                Id = Id,
                TimeStamp = TimeStamp
            };
        }
    }
}
