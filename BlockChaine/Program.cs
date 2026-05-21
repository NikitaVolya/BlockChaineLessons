
using BlockChaine.Consensus;
using BlockChaine.Models;
using BlockChaine.Services;
using System;


var consensuRule = new POWConsesnsusRule(2);
var blockchain = new BlockChainService(consensuRule);
var walletService = new WalletService();
var transactionService = new TransactionService(walletService);


var aliceWallet = walletService.CreateWallet("Alice");
var bobWallet = walletService.CreateWallet("Bob");

var tx1 = transactionService.CreateTransaction(aliceWallet, bobWallet.Address, 10, "");
var tx2 = transactionService.CreateTransaction(bobWallet, aliceWallet.Address, 5, "");

tx1.Signature[3] = 5;

try
{
    blockchain.AddBlock(
        new List<Transaction> { tx1, tx2 }
    );
} catch (Exception ex)
{
    Console.BackgroundColor = ConsoleColor.Red;
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine($"Error adding block: {ex.Message}");
    Console.ResetColor();
}



var displayService = new BlockChainDisplayService();

displayService.PrintWallet(aliceWallet);
displayService.PrintWallet(bobWallet);

displayService.DisplayBlockChain(blockchain.Chain);
displayService.PrintChainValidity(blockchain.isValid().Item1);