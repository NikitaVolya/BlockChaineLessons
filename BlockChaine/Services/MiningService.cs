using BlockChaine.Consensus;

namespace BlockChaine.Services
{

    internal class MiningService
    {
        private readonly HashingService _hashingService;
        private readonly IConsesnsusRule _consesnsusRule;

        public MiningService(IConsesnsusRule consesnsusRule)
        {
            _hashingService = new HashingService();
            _consesnsusRule = consesnsusRule;
        }

        public void MineBlock(Models.Block block)
        {
            while (true)
            {
                block.Hash = _hashingService.ComputeHash(block);
                if (_consesnsusRule.IsValid(block.Hash))
                {
                    break;
                }
                block.Nonce++;
            }
        }
    }
}
