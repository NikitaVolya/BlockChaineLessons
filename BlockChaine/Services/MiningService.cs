using BlockChaine.Consensus;
using System.Security.Cryptography;
using System.Text;

namespace BlockChaine.Services
{

    internal class MiningService
    {
        private readonly HashingService _hashingService;
        private readonly IConsesnsusRule _consesnsusRule;

        private object _lock = new object();
        private volatile int _nonceValue = 0;
        private static int _taskCount = 16;



        public MiningService(IConsesnsusRule consesnsusRule)
        {
            _hashingService = new HashingService();
            _consesnsusRule = consesnsusRule;
        }
        public static void SetTaskCount(int count)
        {
            if (count >= 1)
            {
                _taskCount = count;
            }
        }

        private void NonceTask(Models.Block block, int start, int step)
        {
            int nonce = start;

            byte[] staticBytes = Encoding.UTF8.GetBytes(_hashingService.ComputeStringHashWitoutNonce(block));
            byte[] buffer = new byte[staticBytes.Length + sizeof(int)];
            byte[] hash = new byte[32];

            Buffer.BlockCopy(staticBytes, 0, buffer, 0, staticBytes.Length);

            using SHA256 sha256 = SHA256.Create();

            while (_nonceValue == 0)
            {
                BitConverter.TryWriteBytes(
                    buffer.AsSpan(staticBytes.Length),
                    nonce
                );

                sha256.TryComputeHash(
                    buffer,
                    hash,
                    out _
                );

                if (_consesnsusRule.IsValid(hash))
                {
                    break;
                }
                nonce += step;
            }
            

            lock (_lock)
            {
                if (_nonceValue == 0)
                    _nonceValue = nonce;
            }
        }

        public void MineBlock(Models.Block block)
        {
            _nonceValue = 0;
            for (int i = 0; i < _taskCount; i++)
            {
                int start = i;
                Task.Run(() => NonceTask(block, start, _taskCount));
            }
            while (_nonceValue == 0)
            {
                Thread.Sleep(100);
            }

            block.Nonce = _nonceValue;
            block.Hash = _hashingService.ComputeHash(block);
        }
    }
}
