
using BlockChaine.Consensus;
using BlockChaine.Models;
using BlockChaine.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;


var consensuRule = new POWConsesnsusRule(5);
var blockchain = new BlockChainService(consensuRule);
var walletService = new WalletService();
var transactionService = new TransactionService(walletService);
var displayService = new BlockChainDisplayService();
var merkleTreeAudito = new MerkleTreeAudito();

var aliceWallet = walletService.CreateWallet("Alice");
var bobWallet = walletService.CreateWallet("Bob");
var johnWallet = walletService.CreateWallet("John");

Console.Write("Enter port number for P2P Network Service: ");
int myport = int.Parse(Console.ReadLine() ?? "0");
Console.Write("Enter port number of a peer to connect: ");
int nodePort = int.Parse(Console.ReadLine() ?? "0");

var p2pNetworkService = new P2PNetworkService(myport, blockchain, new List<PeerInfo> { new PeerInfo("localhost", nodePort) });

Block block;


Wallet SelectWallet(string? name)
{
    if (string.IsNullOrEmpty(name))
    {
        Console.WriteLine("Wallet name cannot be empty. Please try again.");
        throw new Exception("Wallet name cannot be empty");
    }
    if (name.ToLower() == aliceWallet.Name.ToLower())
    {
        return aliceWallet;
    }
    else if (name.ToLower() == bobWallet.Name.ToLower())
    {
        return bobWallet;
    }
    else if (name.ToLower() == johnWallet.Name.ToLower())
    {
        return johnWallet;
    }
    else
    {
        Console.WriteLine("Invalid wallet name. Please try again.");
        throw new Exception("Invalid wallet name");
    }
}

void CheckTampering(Block block)
{
    TamperDetectorService tamperDetector = new TamperDetectorService();

    Transaction randomTransaction = block.Transactions[new Random().Next(block.Transactions.Count)];

    tamperDetector.DetectTampering(block, randomTransaction);
}

while (true)
{
    string? senderName, recipientName, memo, recipientAddress, txId;
    decimal amount, fee;
    int lockTime;
    Wallet senderWallet;

    Console.WriteLine("0. Create sample transactions");
    Console.WriteLine("1. Create transaction");
    Console.WriteLine("2. Mine pending transactions");
    Console.WriteLine("3. Display blockchain");
    Console.WriteLine("4. Get balance");
    Console.WriteLine("5. Change transaction");
    Console.WriteLine("6. Display pending transactions");
    Console.WriteLine("7. Display Merkle Tree for block");
    Console.WriteLine("8. Check tampering");
    Console.WriteLine("9. Chow transaction confirmation");
    Console.WriteLine("10. Start P2P Network Service");
    Console.WriteLine("11. Exit");

    var choice = Console.ReadLine();
    switch (choice)
    {
        case "0":
            blockchain.MinePendingTransactions(bobWallet.Address);
            blockchain.MinePendingTransactions(bobWallet.Address);
            blockchain.MinePendingTransactions(johnWallet.Address);
            blockchain.MinePendingTransactions(bobWallet.Address);
            var tx1 = transactionService.CreateTransaction(bobWallet, aliceWallet.Address, 20, "Payment for services", 10);
            var tx2 = transactionService.CreateTransaction(bobWallet, johnWallet.Address, 10, "Gift", 10);
            blockchain.AddTransaction(tx1);
            blockchain.AddTransaction(tx2);
            blockchain.MinePendingTransactions(johnWallet.Address);
            Console.WriteLine("Sample transactions created successfully!");
            break;
        case "1":
            Console.Write("Enter sender (Alice/Bob/John): ");
            senderName = Console.ReadLine();
            Console.Write("Enter recipient (Alice/Bob/John): ");
            recipientName = Console.ReadLine();
            Console.Write("Enter amount: ");
            amount = decimal.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Fee: ");
            fee = decimal.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter memo: ");
            memo = Console.ReadLine() ?? "";
            Console.Write("LockTime: ");
            lockTime = int.Parse(Console.ReadLine() ?? "0");

            senderWallet = SelectWallet(senderName);
            recipientAddress = SelectWallet(recipientName).Address;
            try
            {
                var transaction = transactionService.CreateTransaction(senderWallet, recipientAddress, amount, memo, fee);

                blockchain.AddTransaction(transaction, lockTime);
                Console.WriteLine("Transaction created successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating transaction: \n{ex.Message}");
            }
            break;
        case "2":
            Console.Write("Enter miner address (Alice/Bob/John): ");
            var minerName = Console.ReadLine();
            string minerAddress = SelectWallet(minerName).Address;

            block = blockchain.MinePendingTransactions(minerAddress);
            p2pNetworkService.BroadcastBlockAsync(block).Wait();

            Console.WriteLine("Block mined successfully!");
            break;
        case "3":
            displayService.DisplayBlockChain(blockchain.Chain);
            displayService.PrintChainValidity(blockchain.isValid().result);
            break;
        case "4":
            Console.Write("Enter wallet name (Alice/Bob/john): "); 
            var walletName = Console.ReadLine();
            Wallet wallet = SelectWallet(walletName);
            var balance = blockchain.GetPendingBalance(wallet.Address);
            displayService.PrintWallet(wallet);
            Console.WriteLine($"Balance for {wallet.Name}: {balance}");
            break;
        case "5":
            Console.Write("Enter transaction ID to change: ");
            txId = Console.ReadLine() ?? "";
            Console.Write("Enter sender (Alice/Bob/John): ");
            senderName = Console.ReadLine();
            Console.Write("Enter recipient (Alice/Bob/John): ");
            recipientName = Console.ReadLine();
            Console.Write("Enter amount: ");
            amount = decimal.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter Fee: ");
            fee = decimal.Parse(Console.ReadLine() ?? "0");
            Console.Write("Enter memo: ");
            memo = Console.ReadLine() ?? "";
            Console.Write("LockTime: ");
            lockTime = int.Parse(Console.ReadLine() ?? "0");

            senderWallet = SelectWallet(senderName);
            recipientAddress = SelectWallet(recipientName).Address;
            try
            {
                var transaction = transactionService.CreateTransaction(senderWallet, recipientAddress, amount, memo, fee);
                transaction.ReplaceTxId = txId;

                blockchain.AddTransaction(transaction, lockTime);
                Console.WriteLine("Transaction created successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating transaction: \n{ex.Message}");
            }
            break;
        case "6":
            displayService.PrintPendingTransactions(blockchain.PendingTransactions);
            break;
        case "7":
            Console.Write("Enter block index: ");
            int blockIndex = int.Parse(Console.ReadLine() ?? "0");

            block = blockchain.Chain.FirstOrDefault(b => b.Index == blockIndex);
            if (block != null)
            {
                List<List<string>> merkleTree = merkleTreeAudito.BuildFullTree(block.Transactions);
                displayService.PrintTreeStructure(merkleTree);
            }
            else
            {
                Console.WriteLine("Block not found.");
            }
            break;
        case "8":
            Console.Write("Enter block index to check for tampering: ");
            int tamperBlockIndex = int.Parse(Console.ReadLine() ?? "0");

            var tamperBlock = blockchain.Chain.FirstOrDefault(b => b.Index == tamperBlockIndex);

            if (tamperBlock != null)
            {
                CheckTampering(tamperBlock);
                List<List<string>> merkleTree = tamperBlock.MerkleTree;

                merkleTree.ForEach(level => Console.WriteLine(string.Join(" ", level)));

                merkleTree[0].ForEach(hash => { 

                    List<MerkleProofElement> proof = merkleTreeAudito.GenerateMerkleProof(merkleTree, hash);

                    Console.Write("Proof for transaction hash: " + hash + " -> ");
                    proof.ForEach(proofHash => Console.Write(proofHash.Hash + " "));

                    if (merkleTreeAudito.VerifyMerkleProof(hash, proof, tamperBlock.MerkleRoot))
                    {
                        Console.WriteLine(" - Valid proof");
                    }
                    else
                    {
                        Console.WriteLine(" - Invalid proof");
                    }

                    Console.WriteLine();
                });
            }
            else
            {
                Console.WriteLine("Block not found.");
            }
            break;
        case "9":
            string transactionId;

            Console.Write("Enter transaction ID to check confirmations: ");
            transactionId = Console.ReadLine() ?? "";
            var confirmations = blockchain.GetTransactionConfirmations(transactionId);

            if (confirmations >= 0)
            {
                Console.WriteLine($"Transaction {transactionId} has {confirmations} confirmations.");
            }
            else
            {
                Console.WriteLine("Transaction not found.");
            }

            break;
        case "10":
            p2pNetworkService.Start();
            break;
        case "11":
            return;
        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;
    }
    Console.WriteLine("Press Enter to continue...");
    Console.ReadLine();
}