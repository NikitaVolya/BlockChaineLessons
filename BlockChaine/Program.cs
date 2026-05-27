
using BlockChaine.Consensus;
using BlockChaine.Models;
using BlockChaine.Services;
using System;
using System.ComponentModel.DataAnnotations;


var consensuRule = new POWConsesnsusRule(4);
var blockchain = new BlockChainService(consensuRule);
var walletService = new WalletService();
var transactionService = new TransactionService(walletService);
var displayService = new BlockChainDisplayService();

var aliceWallet = walletService.CreateWallet("Alice");
var bobWallet = walletService.CreateWallet("Bob");
var johnWallet = walletService.CreateWallet("John");

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

while (true)
{
    string? senderName, recipientName, memo, recipientAddress, txId;
    decimal amount, fee;
    int lockTime;
    Wallet senderWallet;

    Console.WriteLine("1. Create transaction");
    Console.WriteLine("2. Mine pending transactions");
    Console.WriteLine("3. Display blockchain");
    Console.WriteLine("4. Get balance");
    Console.WriteLine("5. Change transaction");
    Console.WriteLine("6. Display pending transactions");
    Console.WriteLine("7. Exit");

    var choice = Console.ReadLine();
    switch (choice)
    {
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
                transaction.LockTime = lockTime;

                blockchain.AddTransaction(transaction);
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
            blockchain.MinePendingTransactions(minerAddress);
            Console.WriteLine("Block mined successfully!");
            break;
        case "3":
            displayService.DisplayBlockChain(blockchain.Chain);
            displayService.PrintChainValidity(blockchain.isValid().isValid);
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
                transaction.LockTime = lockTime;

                blockchain.AddTransaction(transaction);
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
            return;
        default:
            Console.WriteLine("Invalid choice. Please try again.");
            break;
    }
    Console.ReadLine();
}