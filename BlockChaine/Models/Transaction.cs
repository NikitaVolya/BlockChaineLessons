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

        public Transaction(string from, string to, decimal amount)
        {
            Id = Guid.NewGuid().ToString();
            From = from;
            To = to;
            Amount = amount;
            TimeStamp = DateTime.UtcNow;
        }

        public string ToRowString()
        {
            return $"{Id}\t{From}\t{To}\t{Amount}\t{TimeStamp}";
        }

        public override string ToString()
        {
            return $"Transaction ID: {Id}\nFrom: {From}\nTo: {To}\nAmount: {Amount}\nTimeStamp: {TimeStamp}";
        }

        public object Clone()
        {
            return new Transaction(From, To, Amount)
            {
                Id = Id,
                TimeStamp = TimeStamp
            };
        }
    }
}
