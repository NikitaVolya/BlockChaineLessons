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

        public string Memo { get; set; }

        public byte[] SenderPublicKey { get; set; }
        public byte[]? Signature { get; set; }

        public Transaction(string from, string to, decimal amount, string memo)
        {
            Id = Guid.NewGuid().ToString();
            From = from;
            To = to;
            Amount = amount;
            Memo = memo;
            TimeStamp = DateTime.UtcNow;
        }

        public byte[] GetDataToSign()
        {
            var data = $"{From}|{To}|{Amount}|{Memo}|{TimeStamp}";
            return Encoding.UTF8.GetBytes(data);
        }

        public string ToRowString()
        {
            string signatureHex = Signature != null ? Convert.ToHexString(Signature) : String.Empty;
            return $"{Id}\t{From}\t{To}\t{Amount}\t{Memo}\t{TimeStamp}\t{signatureHex}";
        }

        public override string ToString()
        {
            return $"Transaction ID: {Id}\nFrom: {From}\nTo: {To}\nAmount: {Amount}\nMemo: {Memo}\nTimeStamp: {TimeStamp}";
        }

        public object Clone()
        {
            return new Transaction(From, To, Amount, Memo)
            {
                Id = Id,
                TimeStamp = TimeStamp
            };
        }
    }
}
