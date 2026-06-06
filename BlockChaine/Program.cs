
using BlockChaine.Consensus;
using BlockChaine.Models;
using BlockChaine.Services;


Console.Write("Enter port number for P2P Network Service (this number will use like node number for local save): ");
int myport = int.Parse(Console.ReadLine() ?? "0");


var consensuRule = new POWConsesnsusRule(5);
var fileService = new FileService(myport);
var blockchain = new BlockChainService(consensuRule, fileService);
var walletService = new WalletService();
var transactionService = new TransactionService(walletService);
var displayService = new BlockChainDisplayService();
var merkleTreeAudito = new MerkleTreeAudito();
var blockChainExplorerService = new BlockChainExplorerService(blockchain);

var aliceWallet = walletService.CreateWallet("Alice");
var bobWallet = walletService.CreateWallet("Bob");
var johnWallet = walletService.CreateWallet("John");


P2PNetworkService p2pNetworkService = new P2PNetworkService(myport, blockchain, new List<PeerInfo> { });

