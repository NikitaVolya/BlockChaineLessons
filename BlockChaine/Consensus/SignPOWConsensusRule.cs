
namespace BlockChaine.Consensus
{
    internal class SignPOWConsensusRule : IConsesnsusRule
    {
        private int _difficulty;
        private string _sign;
        private Int16[] _signBytes;

        public SignPOWConsensusRule(int difficulty, string sign)
        {
            _difficulty = difficulty;
            if (!IsValidSign(sign))
            {
                throw new ArgumentException("Invalid sign. Sign must be a hexadecimal string (characters 0-9 and a-f).");
            }
            _signBytes = Array.Empty<Int16>();
            _sign = "";
            Sign = sign;
        }

        public string Sign
        {
            get { return _sign; }
            set {                
                if (!IsValidSign(value)) {
                    throw new ArgumentException("Invalid sign. Sign must be a hexadecimal string (characters 0-9 and a-f).");
                }

                _sign = value.ToLower();
                _signBytes = new Int16[_sign.Length];
                for (int i = 0; i < _sign.Length; i++)
                {
                    if ('0' < _sign[i] && _sign[i] < '9')
                        _signBytes[i] = (Int16)(_sign[i] - '0');
                    else
                        _signBytes[i] = (Int16)(_sign[i] - 'a' + 10);
                }
            }
        }

        private bool IsValidSign(string sign)
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

        public bool IsValid(byte[] hash)
        {
            int index, shift, i, j;
            Int16 element;

            bool res = true;

            for (i = 0; i < _difficulty; i++)
            {
                index = i / 2;
                shift = ((i + 1) % 2) * 4;
                if ((hash[index] & (15 << shift)) > 0)
                {
                    res = false;
                    break;
                }
            }

            if (res)
            {
                for (j = 0; j < _signBytes.Length; j++)
                {
                    index = (i + j) / 2;
                    shift = ((i + j + 1) % 2) * 4;
                    element = (Int16)((hash[index] >> shift) & 15);
                    if (element != _signBytes[j])
                    {
                        res = false;
                        break;
                    }
                }
            }
            return res;
        }

        public void AddDificulty(int value)
        {
            _difficulty += value;
            if (_difficulty < 0)
                _difficulty = 0;
        }
        public int GetDificulty() { 
            return _difficulty;
        }

        public object Clone()
        {
            return new SignPOWConsensusRule(_difficulty, _sign);
        }
    }
}
