

namespace BlockChaine.Consensus
{
    internal class BaseConsesnsusRule : IConsesnsusRule
    {
        private readonly int _difficulty;
        public BaseConsesnsusRule(int difficulty)
        {
            _difficulty = difficulty;
        }
        public bool IsValid(string hash)
        {
            var prefix = new string('0', _difficulty);
            return hash.StartsWith(prefix);
        }
    }
}
