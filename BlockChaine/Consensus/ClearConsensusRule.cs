

namespace BlockChaine.Consensus
{
    internal class ClearConsensusRule : IConsesnsusRule
    {
        public bool IsValid(string hash)
        {
            return true;
        }
    }
}
