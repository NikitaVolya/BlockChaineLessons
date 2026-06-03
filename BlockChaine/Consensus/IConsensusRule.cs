

namespace BlockChaine.Consensus
{
    public interface IConsesnsusRule : ICloneable
    {
        bool IsValid(string hash);

        bool IsValid(string hash, int dificulty);

        bool IsValid(byte[] hash);

        bool IsValid(byte[] hash, int dificulty);

        void AddDificulty(int value);

        int GetDificulty();
    }
}
