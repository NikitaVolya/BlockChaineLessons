using BlockChaine.Models;
using BlockChaine.Consensus;


namespace BlockChaine.Services
{
    internal class BlockChainService
    {
        public List<Block> Chain { get; set; }


        private readonly HashingService _hashingService;
        private readonly MiningService _miningService;
        private readonly IConsesnsusRule _consensusRule;

        public BlockChainService(IConsesnsusRule consesnsusRule)
        {
            Chain = new List<Block>();
            _hashingService = new HashingService();
            _miningService = new MiningService(consesnsusRule);
            _consensusRule = consesnsusRule;
            CreateGenesisBlock();
        }

        private void CreateGenesisBlock()
        {
            var genesisBlock = new Block(0, "", "", "0");
            _miningService.MineBlock(genesisBlock);
            Chain.Add(genesisBlock);
        }

        public void AddBlock(string author, string data)
        {
            var previousBlock = Chain.Last();
            var newBlock = new Block(previousBlock.Index + 1, author, data, previousBlock.Hash);
            _miningService.MineBlock(newBlock);
            Chain.Add(newBlock);
        }

        public int GetCorruptedBlockIndex() {

            for (int i = 1; i < Chain.Count; i++) { 
                
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return i;
                if (currentBlock.PreviousHash != previousBlock.Hash)
                    return i;
            }

            return -1;
        }

        public void HackTheChain(int index) {

            for (int i = index; i < Chain.Count; i++) { 
                
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                currentBlock.PreviousHash = previousBlock.Hash;
                _miningService.MineBlock(currentBlock);
            }

        }

        public bool isChainValid()
        {
            for (int i = 1; i < Chain.Count; i++)
            {
                var currentBlock = Chain[i];
                var previousBlock = Chain[i - 1];

                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return false;
                if (currentBlock.PreviousHash != previousBlock.Hash)
                    return false;
            }
            return true;
        }

        public bool IsValidChain(List<Block> chain)
        {
            for (int i = 1; i < chain.Count; i++)
            {
                var currentBlock = chain[i];
                var previousBlock = chain[i - 1];

                if (currentBlock.Hash != _hashingService.ComputeHash(currentBlock))
                    return false;
                if (currentBlock.PreviousHash != previousBlock.Hash)
                    return false;

            }

            return true;
        }

        public bool ResolveConsensus(List<Block> competingChain)
        {
            Console.WriteLine("Resolving consensus...");

            if (!IsValidChain(competingChain))
            {
                Console.WriteLine("Competing chain is invalid.");
                return false;
            }

            if (competingChain.Count > Chain.Count)
            {
                Console.WriteLine("Competing chain wins. Reorganizing blockchain...");

                Chain = competingChain
                    .Select(block => (Block) block.Clone())
                    .ToList();

                return true;
            }

            Console.WriteLine("Current chain remains authoritative.");
            return false;
        }
    }
}
