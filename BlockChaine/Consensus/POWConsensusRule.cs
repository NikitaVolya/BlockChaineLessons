

namespace BlockChaine.Consensus
{
    internal class POWConsesnsusRule : IConsesnsusRule
    {
        private int _difficulty;

        public POWConsesnsusRule(int difficulty)
        {
            _difficulty = difficulty;
        }
        public bool IsValid(string hash)
        {
            return IsValid(hash, _difficulty);
        }

        public bool IsValid(string hash, int difficulty)
        {
            var prefix = new string('0', difficulty);
            return hash.StartsWith(prefix);
        }

        public bool IsValid(byte[] hash)
        {
            return IsValid(hash, _difficulty);
        }

        public bool IsValid(byte[] hash, int difficulty)
        {
            int index, shift;
            bool res = true;

            for (int i = 0; i < difficulty; i++)
            {
                index = i / 2;
                shift = ((i + 1) % 2) * 4;
                if ((hash[index] & (15 << shift)) > 0)
                {
                    res = false;
                    break;
                }
            }

            return res;
        }

        public void AddDificulty(int value)
        {
            _difficulty += value;
            if (_difficulty < 0)
            {
                _difficulty = 0;
            }
        }
        public int GetDificulty()
        {
            return _difficulty;
        }

        public object Clone()
        {
            return new POWConsesnsusRule(_difficulty);
        }
    }
}
