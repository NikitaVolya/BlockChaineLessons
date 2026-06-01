using BlockChaine.Models;

namespace BlockChaine.Services
{
    public enum TreePosition
    {
        Left,
        Right
    }

    public record MerkleProofElement(string Hash, TreePosition Position);


    public class MerkleTreeAudito
    {
        public List<List<string>> BuildFullTree(List<Transaction> transactions)
        {
            HashingService hashingService = new HashingService();

            List<List<string>> tree = new List<List<string>>() {
                transactions.Select(tx => hashingService.ComputeSha256Hash(tx.ToRowString())).ToList()
            };

            while (tree.Last().Count > 1)
            {

                List<string> currentLevel = tree.Last();
                List<string> newLevel = new List<string>();


                for (int i = 0; i < currentLevel.Count - (currentLevel.Count % 2 == 0 ? 0 : 1); i += 2)
                {
                    string combinedHash = currentLevel[i] + currentLevel[i + 1];
                    newLevel.Add(hashingService.ComputeSha256Hash(combinedHash));
                }

                if (currentLevel.Count % 2 != 0)
                    newLevel.Add(currentLevel.Last());

                tree.Add(newLevel);
            }

            return tree;
        }

        public List<MerkleProofElement> GenerateMerkleProof(List<List<string>> tree, string transactionHash)
        {
            int transactionIndex = 0;
            for (; transactionIndex < tree[0].Count; transactionIndex++)
            {
                if (tree[0][transactionIndex] == transactionHash)
                    break;
            }

            if (transactionIndex == tree[0].Count)
                throw new Exception("Transaction hash not found in the tree");

            List<MerkleProofElement> proof = new List<MerkleProofElement>();

            int levelIndex, neaborIndex;

            levelIndex = 0;

            MerkleProofElement? proofElement = null;

            do
            {

                if (transactionIndex % 2 == 0)
                {
                    neaborIndex = transactionIndex + 1;
                    if (neaborIndex < tree[levelIndex].Count)
                    {
                        proofElement = new MerkleProofElement(tree[levelIndex][neaborIndex], TreePosition.Right);
                    }
                }
                else
                {
                    neaborIndex = transactionIndex - 1;
                    proofElement = new MerkleProofElement(tree[levelIndex][neaborIndex], TreePosition.Left);
                }

                if (proofElement != null)
                    proof.Add(proofElement);

                levelIndex++;
                transactionIndex /= 2;

            } while (levelIndex < tree.Count - 1);

            return proof;
        }

        public bool VerifyMerkleProof(string transactionHash, List<MerkleProofElement> proof, string rootHash)
        {
            HashingService hashingService = new HashingService();

            string computedHash = transactionHash;

            foreach (MerkleProofElement proofElement in proof)
            {
                if (proofElement.Position == TreePosition.Left)
                    computedHash = hashingService.ComputeSha256Hash(proofElement.Hash + computedHash);
                else
                    computedHash = hashingService.ComputeSha256Hash(computedHash + proofElement.Hash);
            }
            return computedHash == rootHash;
        }
    }
}
