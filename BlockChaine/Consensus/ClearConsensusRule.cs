

namespace BlockChaine.Consensus
{
    internal class ClearConsensusRule : IConsesnsusRule
    {
        public bool IsValid(string hash)
        {
            return true;
        }

        public bool IsValid(string hash, int dificulty)
        {
            return true;
        }

        public bool IsValid(byte[] hash)
        {
            return true;
        }

        public bool IsValid(byte[] hash, int dificulty)
        {
            return true;
        }

        public void AddDificulty(int value)
        {

        }

        public int GetDificulty()
        {
            return 0;
        }

        public object Clone()
        {
            return new ClearConsensusRule();
        }
    }
}
