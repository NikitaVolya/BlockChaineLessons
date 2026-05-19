using BlockChaine.Consensus;
using BlockChaine.Models;
using BlockChaine.Services;

namespace BlockChaine.Lessons
{
    internal class Lesson4
    {
        private static void HackFirstBlock(BlockChainService blockchain)
        {
            var hashingService = new HashingService();

            var firstBlock = blockchain.Chain[1];
            firstBlock.Transactions[0].Amount = 1000;

            firstBlock.Hash = hashingService.ComputeHash(firstBlock);

            // Взлом першого блоку призводить до перерахування його хешу, через що
            // у результаті змінюється хеш другого блоку, потім третього і так далі, що робить увесь ланцюжок недійсним.
        }


        public static void ClassWork()
        {
            var blockchain = new BlockChainService(new POWConsesnsusRule(4));
            var transactionService = new TransactionService();
            var display = new BlockChainDisplayService();

            blockchain.AddBlock(new List<Models.Transaction>()
            {
                transactionService.CreateTransaction("Alice", "Bob", 5),
            });
            blockchain.AddBlock(new List<Models.Transaction>()
            {
                transactionService.CreateTransaction("Bob", "Charlie", 2),
                transactionService.CreateTransaction("Alice", "Charlie", 1),
            });
            blockchain.AddBlock(new List<Models.Transaction>()
            {
                transactionService.CreateTransaction("Charlie", "Alice", 3),
                transactionService.CreateTransaction("Bob", "Alice", 1),
            });

            try
            {
                blockchain.AddBlock(new List<Models.Transaction>()
                {
                    transactionService.CreateTransaction("Alice", "Bob", 4),
                    transactionService.CreateTransaction("Charlie", "Bob", 2),
                    transactionService.CreateTransaction("Bob", "Charlie", 1),
                    transactionService.CreateTransaction("Alice", "Charlie", 2),
                    transactionService.CreateTransaction("Charlie", "Alice", 1),
                });
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.WriteLine($"Error: {ex.Message}\n\n");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }

            display.DisplayBlockChain(blockchain.Chain);
            display.PrintChainValidity(blockchain.isValid());

            Console.WriteLine("\n\nHacker Attack\n");
            HackFirstBlock(blockchain);

            display.DisplayBlockChain(blockchain.Chain);
            display.PrintChainValidity(blockchain.isValid());
        }

        public static void HomeWork()
        {
            /*
             * #### Рівень 1 (Обов'язковий): Історія переказів (Transaction Explorer)
             * **Суть:** Користувач хоче знати, куди поділися його гроші та від кого він отримував перекази. Для цього системі потрібен інструмент пошуку.
             * 
             * **Ваше завдання:**
             * Створити метод візуалізації історії для конкретної адреси (користувача).
             * 
             * 1. Додайте у ваш клас, який відповідає за вивід у консоль (Display), новий метод `PrintTransactionHistory(string address)`.
             * 2. Метод має пройтися абсолютно по всіх блоках у ланцюзі (крім Genesis, де транзакцій немає) і по всіх транзакціях усередині цих блоків.
             * 3. Якщо передана адреса збігається з відправником (`From`) АБО з отримувачем (`To`) — виведіть цю транзакцію на екран.
             * 4. **Що здавати:** Покажіть консоль, де викликано пошук, наприклад, для "Аліси". На екрані має бути акуратний список: у якому блоці, кому вона відправляла монети, і від кого отримувала.
             * Якщо запитати історію неіснуючого користувача (наприклад, "Бетмен") — система має коректно написати "Транзакцій не знайдено".
             */

            var blockchain = new BlockChainService(new POWConsesnsusRule(4));
            var transactionService = new TransactionService();
            var display = new BlockChainDisplayService();

            blockchain.AddBlock(new List<Models.Transaction>()
            {
                transactionService.CreateTransaction("Alice", "Bob", 5),
            });
            blockchain.AddBlock(new List<Models.Transaction>()
            {
                transactionService.CreateTransaction("Bob", "Charlie", 2),
                transactionService.CreateTransaction("Alice", "Charlie", 1),
            });
            blockchain.AddBlock(new List<Models.Transaction>()
            {
                transactionService.CreateTransaction("Charlie", "Alice", 3),
                transactionService.CreateTransaction("Bob", "Alice", 1),
            });


            display.PrintTransactionHistory(blockchain.Chain, "Alice");
            display.PrintChainValidity(blockchain.isValid());

            /*
             * #### Рівень 2 (Поглиблений): Полювання на "Китів" (Whale Tracker)
             * 
             * **Суть:** У світі криптовалют "китами" називають адреси, які переказують гігантські суми, впливаючи на ринок. Аналітики постійно відстежують такі аномальні перекази.
             * **Ваше завдання:**
             * Написати алгоритм пошуку найбільшої транзакції в історії вашої мережі.
             * 
             * 1. Створіть метод, який аналізує весь ланцюг і знаходить **одну єдину транзакцію** з найбільшим показником `Amount` за весь час існування блокчейну.
             * 2. Враховуйте, що транзакцій можуть бути тисячі, розкиданих по сотнях блоків. Подумайте, як ефективно знайти максимум (можете використати класичні цикли або спробувати написати красивий LINQ-запит).
             * 3. **Що здавати:** Запустіть програму з 4-5 блоками, де є багато різних переказів. Викличте ваш метод. У консолі має з'явитися окремий блок тексту: *"🏆 Найбільша транзакція в мережі: Блок #X | Відправник -> Отримувач | Сума"*.
             */
            Transaction? largestTransaction = transactionService.FindLargestTransaction(blockchain.Chain);
            if (largestTransaction != null)
            {
                Console.WriteLine($"\nLargest transaction in network: From: {largestTransaction.From} -> To: {largestTransaction.To} | Amount: {largestTransaction.Amount}");
            }
            else
            {
                Console.WriteLine("\nNo transactions found.");
            }
        }
    }
}