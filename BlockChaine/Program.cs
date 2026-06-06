
using BlockChaine.Consensus;
using BlockChaine.Models;
using BlockChaine.Services;
using System.Runtime.InteropServices;


Console.Write("Enter port number for P2P Network Service (this number will use like node number for local save): ");
int myport = int.Parse(Console.ReadLine() ?? "0");


var consensuRule = new SignPOWConsensusRule(4, "bbf");
var fileService = new FileService(myport);

var blockchain = new BlockChainService(consensuRule, fileService);
fileService.SaveChain(blockchain.Chain);

var walletService = new WalletService();
var walletKeystoreService = new WalletKeystoreService();
var transactionService = new TransactionService(walletService);
var displayService = new BlockChainDisplayService();
var merkleTreeAudito = new MerkleTreeAudito();
var blockChainExplorerService = new BlockChainExplorerService(blockchain);


P2PNetworkService p2pNetworkService = new P2PNetworkService(myport, blockchain, new List<PeerInfo> { });


(Wallet?, bool) CreateNewWallet()
{
    Console.Write("Enter wallet name: ");
    string walletName = Console.ReadLine() ?? "";

    string password, passwordConfirmation;
    do
    {
        Console.Write("Enter wallet password: ");
        password = Console.ReadLine() ?? "";

        Console.Write("Enter wallet password confirmation: ");
        passwordConfirmation = Console.ReadLine() ?? "";

        if (passwordConfirmation != password)
        {
            Console.WriteLine("Passwords are differents");
        }
    } while (password != passwordConfirmation);

    Console.Write("Enter wallet file name[default wallet.dat]: ");
    string? walletPath = Console.ReadLine();
    if (walletPath == null || walletPath.Trim() == "")
    {
        walletPath = WalletKeystoreService.DefaultFileName;
    }

    Wallet tmpWallet = walletService.CreateWallet(walletName);
    try
    {
        walletKeystoreService.SaveWallet(tmpWallet, password, walletPath);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        return (null, false);
    }

    Console.WriteLine("Wallet created and saved successfully.");
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();

    return (tmpWallet, true);
}

(Wallet?, bool) LoadDefaultWallet()
{
    if (File.Exists(WalletKeystoreService.DefaultFileName))
    {
        Console.Write("Enter wallet password: ");
        string password = Console.ReadLine() ?? "";
        try
        {
            Wallet wallet = walletKeystoreService.LoadWalletFromFile(password, WalletKeystoreService.DefaultFileName);

            Console.WriteLine("Wallet loaded successfully.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            return (wallet, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Console.WriteLine("Failed to load wallet. Press any key to continue...");
            Console.ReadKey();
        }
    }
    else
    {
        Console.WriteLine("Default wallet file not found. Press any key to continue...");
        Console.ReadKey();
    }

    return (null, false);
}

(Wallet?, bool) LoadWalletFromPath()
{
    Console.Write("Enter wallet file path: ");
    string? walletPath = Console.ReadLine();
    if (walletPath == null || walletPath.Trim() == "")
    {
        Console.WriteLine("Invalid file path. Press any key to continue...");
        Console.ReadKey();
        return (null, false);
    }

    if (File.Exists(walletPath))
    {
        Console.Write("Enter wallet password: ");
        string password = Console.ReadLine() ?? "";
        try
        {
            Wallet wallet = walletKeystoreService.LoadWalletFromFile(password, walletPath);

            Console.WriteLine("Wallet loaded successfully.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            return (wallet, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Console.WriteLine("Failed to load wallet. Press any key to continue...");
            Console.ReadKey();
        }
    }
    else
    {
        Console.WriteLine("Wallet file not found. Press any key to continue...");
        Console.ReadKey();
    }

    return (null, false);
}

Wallet? LoginHandler()
{ 
    Wallet? wallet = null;
    bool close = false;

    UserInterfaceService loginScreen = new UserInterfaceService(new List<(string, Action)>()
    {
        ("Create new wallet", () => {
            (wallet, close) = CreateNewWallet();
        }),
        ("Load default wallet", () => {
            (wallet, close) = LoadDefaultWallet();
        }),
        ("Load wallet from filepath", () => {
            (wallet, close) = LoadWalletFromPath();
        }),
        ("Exit", () => { close = true; })
    });

    while (!close)
    {
        loginScreen.ShowOptionDialog();
    }

    return wallet;
}

void ShowWalletBalance(Wallet wallet)
{
    Console.Write("Choice token[or default token MAIN]: ");
    string? token = Console.ReadLine();

    if (token == null || token.Trim() == "")
    {
        token = Transaction.MAIN_TOKEN_SYMBOL;
    }

    Console.WriteLine("Pending balance: " + blockchain.GetPendingBalance(wallet.Address, token));
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();

}

void ShowWalletTransactions(Wallet wallet)
{
    Console.WriteLine(new string('=', 50));

    displayService.PrintWallet(wallet);
    Console.WriteLine("Transactions: ");


    List<Transaction> transactions = blockChainExplorerService.GetTransactionHistory(wallet.Address);

    Console.WriteLine(new string('=', 50));

    if (transactions.Count == 0)
        Console.WriteLine("No transaction found");

    foreach (Transaction transaction in transactions)
    {
        Console.WriteLine(transaction);
    }
    Console.WriteLine(new string('=', 50));

    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}

void CreateTransaction(Wallet wallet)
{
    Console.Write("Choice token[or default token MAIN]: ");
    string? token = Console.ReadLine();

    if (token == null || token.Trim() == "")
    {
        token = Transaction.MAIN_TOKEN_SYMBOL;
    }

    Console.Write("Enter recipient address: ");
    string address = Console.ReadLine() ?? "";

    Console.Write("Enter amount: ");
    decimal amount = decimal.Parse(Console.ReadLine() ?? "0");
    Console.Write("Enter Fee: ");
    decimal fee = decimal.Parse(Console.ReadLine() ?? "0");
    Console.Write("Enter memo: ");
    string memo = Console.ReadLine() ?? "";

    try
    {
        var transaction = transactionService.CreateTransaction(wallet, address, amount, memo, fee, token);

        blockchain.AddTransaction(transaction);
        Console.WriteLine("Transaction created successfully!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error creating transaction: \n{ex.Message}");
    }
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}

void MinePendingTransactions(Wallet wallet)
{
    Block block = blockchain.MinePendingTransactions(wallet.Address);

    p2pNetworkService.BroadcastBlockAsync(block).Wait();

    Console.WriteLine("Block mined successfully!");
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}

void AddPeerByPort()
{
    int port;
    Console.Write("Enter port to node: ");
    port = int.Parse(Console.ReadLine() ?? "0");

    p2pNetworkService.AddPeerByPort(port).Wait();
}

void MintingNewToken(Wallet wallet)
{
    Console.Write("Enter name of token: ");
    string? tokenName = Console.ReadLine();

    if (tokenName == null || tokenName.Trim() == "")
    {
        Console.WriteLine("Invalide input!");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        return;
    }

    Console.Write("Enter amount of token: ");
    decimal amount = decimal.Parse(Console.ReadLine() ?? "1");

    try
    {
        Transaction mintingTransaction = new Transaction(Transaction.MINTING_TOKEN, wallet.Address, amount, "Mainting of new token", 0, tokenName);
        blockchain.AddTransaction(mintingTransaction);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.ToString());
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
        return;
    }
    Console.WriteLine("Minting created successfully!");
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}

void MenuHandler(Wallet wallet)
{
    bool close = false;
    UserInterfaceService menuScreen = new UserInterfaceService(new List<(string, Action)>()
    {
        ("Show balance", () => {
            ShowWalletBalance(wallet);
        }),
        ("Show wallet", () => {
            displayService.PrintWallet(wallet);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }),
        ("Show transactions", () => {
            ShowWalletTransactions(wallet);
        }),
        ("Create new transaction", () => {
            CreateTransaction(wallet);
        }),
        ("Mine pending transactions", () => {
            MinePendingTransactions(wallet);
        }),
        ("Add Node peer by port", () => {
            AddPeerByPort();
        }),
        ("Minting new token", () => {
            MintingNewToken(wallet);
        }),
        ("Display block chain", () => {
            displayService.DisplayBlockChain(blockchain.Chain);

            (bool result, string message) = blockchain.isValid();
            displayService.PrintChainValidity(result);
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }),
        ("Exit", () => { close = true; })
    });

    while (!close)
    {
        menuScreen.ShowOptionDialog();
    }
}




Wallet? wallet = LoginHandler();
p2pNetworkService.Start();
if (wallet != null)
    MenuHandler(wallet);
Console.WriteLine("Good bie");