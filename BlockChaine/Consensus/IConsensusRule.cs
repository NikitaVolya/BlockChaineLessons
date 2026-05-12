

namespace BlockChaine.Consensus
{
    internal interface IConsesnsusRule
    {
        bool IsValid(string hash);
    }
}
