using BlockChaine.Consensus;
using BlockChaine.Models;
using BlockChaine.Services;

namespace BlockChaine.Lessons
{
    internal class Lesson2
    {
        /* 
         * Завдання 1. "Vanity Hash" (Іменна криптографічна печатка) 
         * Рівень 1 (обов'язково) — запустити код, подивитись на таблицю benchmark, переконатись що difficulty=4 помітно повільніше ніж difficulty=2.
         * Рівень 2 (для допитливих) — спробувати difficulty=5. Виміряти час. Подумати: якщо difficulty=6 займає умовно 100 секунд на одному блоці — наскільки складно підробити ланцюг з 10 блоків?
         */
        public static void RunExercice1()
        {
            BlockChainDisplayService display = new BlockChainDisplayService();
            List<int> tests = new List<int> { 2, 3, 4, 5, 6 };
            /*
             * DIFFICULTY 2 time taken for 1 block: 1 ms
             * DIFFICULTY 3 time taken for 1 block: 9 ms
             * DIFFICULTY 4 time taken for 1 block: 177 ms
             * DIFFICULTY 5 time taken for 1 block: 3529 ms
             * DIFFICULTY 6 time taken for 1 block: 57283 ms
            */

            var watch = new System.Diagnostics.Stopwatch();

            foreach (int difficylty in tests)
            {
                Console.Write($"DIFFICULTY {difficylty}");

                IConsesnsusRule consensus = new BaseConsesnsusRule(difficylty);
                BlockChainService mainChain = new BlockChainService(consensus);

                watch.Start();

                mainChain.AddBlock("Alice", "-> Bob: 5 BTC");

                watch.Stop();
                Console.WriteLine($" time taken for 1 block: {watch.ElapsedMilliseconds} ms");
            }

            /*
             * Якщо складність 6 у моєму випадку займає близько 57 секунд,
             * то підробка ланцюга з 10 блоків займе приблизно 570 секунд, або близько 10 хвилин.
             */
        }

        /*
         *  Завдання 2. Симуляція Атаки 51% та Форк-резолвер (Закон найдовшого ланцюга)
         *  Опис
         *  У децентралізованих мережах одночасно можуть існувати дві різні версії правди (форки), якщо два майнери знайшли блок майже одночасно, або якщо зловмисник намагається переписати історію. Блокчейн завжди обирає ту гілку, в яку вкладено більше сумарної роботи (найдовший валідний ланцюг).
         *  Технічні вимоги
         *  Створіть екземпляр Blockchain (Головна гілка) та додайте в нього 4 блоки (Index від 0 до 3).
         *  Створіть сценарій "Атаки": зловмисник копіює стан ланцюга до Блоку #1 (включно) у новий окремий список/ланцюг (Гілка атакуючого).
         *  У гілці атакуючого зловмисник створює альтернативний Блок #2 (з іншими даними, наприклад, "Атакуючий вкрав 1000 монет") і починає посилено майнити нові блоки, щоб обігнати головну гілку. Він майнить альтернативні Блоки #2, #3, #4 та #5.
         */
        public static void RunExercice2()
        {
            var consensus = new BitcoinConsensusRule(2, "cafe");

            var mainChain = new BlockChainService(consensus);
            var display = new BlockChainDisplayService();

            Console.WriteLine("=== MAIN CHAIN ===");

            mainChain.AddBlock("Alice", "-> Bob: 5 BTC");
            mainChain.AddBlock("Bob", "-> Charlie: 2 BTC");
            mainChain.AddBlock("Charlie", "-> Dave: 1 BTC");

            display.DisplayBlockChain(mainChain.Chain);

            Console.WriteLine("\nMAIN CHAIN VALID:");
            display.PrintChainValidity(mainChain.isValid());

            Console.WriteLine("\n=== ATTACKER MINING ===");

            var attackerBlocks = mainChain.Chain
                .Take(2)
                .Select(b => (Block)b.Clone())
                .ToList();

            var attackerChain = new BlockChainService(consensus);
            attackerChain.Chain = attackerBlocks;

            attackerChain.AddBlock("Hacker", "Attacker stole 1000 BTC");
            attackerChain.AddBlock("Alice", "Fake payment #1");
            attackerChain.AddBlock("Bob", "Fake payment #2");
            attackerChain.AddBlock("Alice", "Fake payment #3");

            Console.WriteLine("\nATTACKER CHAIN:");

            display.DisplayBlockChain(attackerChain.Chain);



            Console.WriteLine("\n=== MAIN CHAIN CONTINUES ===");

            mainChain.AddBlock("Alice", "Normal transaction");

            display.DisplayBlockChain(mainChain.Chain);


            Console.WriteLine("\n=== CONSENSUS RESOLUTION ===");

            mainChain.ResolveConsensus(attackerChain.Chain);

            Console.WriteLine("\n=== FINAL AUTHORITATIVE CHAIN ===");

            display.DisplayBlockChain(mainChain.Chain);

            Console.WriteLine("\nCHAIN VALID:");
            display.PrintChainValidity(mainChain.isValid());
        }

        public static void RunHomeWork()
        {
            /*RunExercice1();*/
            RunExercice2();
        }

        public static void Run()
        {
            RunHomeWork();
        }
    }
}
