

namespace BlockChaine.Consensus
{
    internal interface IConsesnsusRule : ICloneable
    {
        bool IsValid(string hash);

        bool IsValid(byte[] hash);

        void AddDificulty(int value);

        int GetDificulty();
    }
}
