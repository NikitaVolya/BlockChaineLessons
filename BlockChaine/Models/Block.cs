using BlockChaine.Consensus;


namespace BlockChaine.Models
{
    public class Block : ICloneable
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public int Nonce { get; set; } = 0;

        public int Dificulty { get; set; } = 0;


        public Block(int index, List<Transaction> transactions, string previousHash)
        {
            Index = index;
            Timestamp = DateTime.UtcNow;
            Transactions = transactions;
            PreviousHash = previousHash;
            Hash = "";
        }

        public object Clone()
        {
            return new Block(Index, Transactions.Select(t => (Transaction)t.Clone()).ToList(), PreviousHash)
            {
                Timestamp = Timestamp,
                Hash = Hash,
                Nonce = Nonce
            };
        }
    }
}
