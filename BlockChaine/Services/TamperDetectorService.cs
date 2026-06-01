using BlockChaine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockChaine.Services
{
    internal class TamperDetectorService
    {
        public void DetectTampering(Block block, Transaction tamperedTransaction)
        {
            tamperedTransaction.Amount += 1000; // Simulate tampering by changing the amount of the transaction

            MerkleTreeAudito merkleTreeAudito = new MerkleTreeAudito();

            List<List<string>> blockMerkcleTree = block.MerkleTree;
            List<List<string>> newMerkleTree = merkleTreeAudito.BuildFullTree(block.Transactions);

            if (blockMerkcleTree.Count != newMerkleTree.Count ||
                (blockMerkcleTree.First().Count != blockMerkcleTree.First().Count))
            {
                Console.WriteLine("Tampering detected: Chanched number of transactions.");
                return;
            }

            Queue<int> queue = new Queue<int>();
            Queue<int> trampedTransactions = new Queue<int>();

            int i = 0;
            int level = blockMerkcleTree.Count - 1;

            const int NEXT_LEVEL_FLAG = -1;

            // Start from the root of the Merkle tree
            queue.Enqueue(0);
            queue.Enqueue(NEXT_LEVEL_FLAG);

            while (queue.Count > 1)
            {
                int x = queue.Dequeue();

                // Change level when we encounter the flag
                if (x == NEXT_LEVEL_FLAG)
                {
                    level--;
                    queue.Enqueue(NEXT_LEVEL_FLAG);
                }
                // Compare the hash at the current position in both trees
                else if (blockMerkcleTree[level][x] != newMerkleTree[level][x])
                {
                    // Check if is a leaf of the Merkle tree
                    if (level == 0)
                    {
                        trampedTransactions.Enqueue(x);
                    }
                    else
                    {
                        queue.Enqueue(2 * x);
                        if (2 * x + 1 < blockMerkcleTree[level - 1].Count)
                            queue.Enqueue(2 * x + 1);
                    }
                }
            }

            if (trampedTransactions.Count > 0)
            {
                Console.WriteLine("Tampering detected: Chanched transactions:");
                while (trampedTransactions.Count > 0)
                {
                    int index = trampedTransactions.Dequeue();
                    Console.WriteLine($"index: {index} (ID: {block.Transactions[index].Id})");
                }
            }
            else
            {
                Console.WriteLine("No tampering detected.");
            }
        }
    }
}
