using BlockChaine.Consensus;


namespace BlockChaine.Models
{
    internal class Block : ICloneable
    {
        public int Index { get; set; }
        public DateTime Timestamp { get; set; }
        public string Author { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
        public string Hash { get; set; }
        public string PreviousHash { get; set; }
        public int Nonce { get; set; } = 0;

        public int Dificulty { get; set; } = 0;

        public IConsesnsusRule? ConsensusRule { get; set; } = null;

        public Block(int index, string author, string data, string previousHash)
        {
            Index = index;
            Timestamp = DateTime.Now;
            Author = author;
            Data = data;
            PreviousHash = previousHash;
            Hash = "";
        }

        public object Clone()
        {
            return new Block(Index, Author, Data, PreviousHash)
            {
                Timestamp = Timestamp,
                Hash = Hash,
                Nonce = Nonce
            };
        }
    }
}
