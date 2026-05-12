
namespace BlockChaine.Consensus
{
    internal class BitcoinConsensusRule : IConsesnsusRule
    {
        private readonly int _difficulty;
        private readonly string _sign;

        public BitcoinConsensusRule(int difficulty, string sign)
        {
            _difficulty = difficulty;
            if (!isValideSign(sign))
            {
                throw new ArgumentException("Invalid sign. Sign must be a hexadecimal string (characters 0-9 and a-f).");
            }
            _sign = sign.ToLower();
        }

        private bool isValideSign(string sign)
        {
            bool res = true;

            sign = sign.ToLower();
            for (int i = 0; i < sign.Length && res; i++)
            {
                /* CHECK IF sign[i] BEETWEN 0 AND 9 OR 'a' AND 'f' */
                if (!(('0' <= sign[i] && sign[i] <= '9') || ('a' <= sign[i] && sign[i] <= 'f')))
                {
                    res = false;
                }
            }
            
            return res;
        }

        public bool IsValid(string hash)
        {
            var prefix = new string('0', _difficulty) + _sign;
            return hash.StartsWith(prefix);
        }
    }
}
