using BlockChaine.Consensus;
using BlockChaine.Services;


namespace BlockChaine.Lessons
{
    internal class Lesson3
    {
        public static void ClassWork()
        {
            var blockchain = new BlockChainService(new POWConsesnsusRule(1));
            var display = new BlockChainDisplayService();

            for (int i = 0; i < 20; i++)
            {

                blockchain.AddBlock("Alice", "-> Bob: 5 BTC");

                display.DisplayBlockChain(blockchain.Chain);
                display.PrintChainValidity(blockchain.isValid());

                Thread.Sleep(1000);
            }
        }

        /*
         * ## Завдання 1: Історія складності (Розминка) 
         * **Мета:** Навчитися фіксувати стан блокчейну на момент створення блоку та відслідковувати зміну складності в мережі. 
         * **Опис завдання:** Додайте до класу `Blockchain` метод `PrintDifficultyHistory()`, який виводить у консоль історію зміни складності (`Difficulty`) по всіх блоках ланцюга. Для цього необхідно навчити блок зберігати значення складності, актуальне саме на момент його майнінгу. 
         */
        /*
         * ## Завдання 2: Паралельний Майнер (Базова оптимізація) 
         * **Мета:** Прискорити підбір `Nonce` за допомогою багатопоточності та ефективного використання всіх ядер процесора. 
         * **Опис завдання:** Наївний алгоритм майнінгу працює в одному потоці, залишаючи решту ядер процесора вільними. Перепишіть процес пошуку `Nonce`, щоб він паралельно утилізував усі доступні ресурси системи. 
         * **Ключові вимоги:** 
         * * **Розподіл діапазонів:** Запустіть паралельний пошук на кількох задачах (`Task`). Кожна задача має перевіряти свій пул значень `Nonce` (наприклад, потік 1 бере числа з кроком `step = 4` починаючи з 0, потік 2 — починаючи з 1 і т.д.). 
         * * **Кооперативне скасування:** Використайте `CancellationTokenSource`. Як тільки один із потоків знаходить валідний хеш, усі інші мають **миттєво** припинити роботу. 
         * * **Потокобезпека:** Гарантуйте, що фінальний запис знайденого `Nonce` та `Hash` у блок не викликає стану перегонів (Race Condition). 
         */
        /*
         * ## Завдання 3: Zero-GC Майнінг (Senior-оптимізація) 
         * **Мета:** Звести до нуля алокації пам'яті в циклі майнінгу та позбавитися навантаження на Garbage Collector. 
         * **Опис завдання:** У базовій реалізації всередині циклу `while` на кожній ітерації створюються нові об'єкти: склеюються рядки (`PreviousHash + Data + Nonce`) та виділяються нові масиви `byte[]` для обчислення хешу. Це створює величезний тиск на підсистему пам'яті. 
         * Позбавтеся **усіх** алокацій пам'яті всередині циклу пошуку: 
         * 1. **Кешування статичних даних:** Склейте незмінну частину блоку (`PreviousHash` + `Timestamp` + `Data`) і перетворіть її на масив байтів **один раз до входу в цикл**. 
         * 2. **Перевикористання буфера:** Створіть один фіксований байтовий масив, куди скопійовано статичні дані, і залишено 4 байти в кінці під `Nonce`. У циклі перезаписуйте лише ці 4 байти через `BitConverter.TryWriteBytes`. 
         * 3. **Пряма перевірка байтів:** Перевіряйте валідність складності (провідні нулі) напряму за байтами згенерованого хешу, замість дорогої конвертації результату в Hex-рядок на кожній ітерації. 
         */
        public static void Exercice12()
        {
            var display = new BlockChainDisplayService();

            var watch = new System.Diagnostics.Stopwatch();

            for (int j = 1; j <= 32; j += 31)
            {
                var blockchain = new BlockChainService(new POWConsesnsusRule(1));

                Console.WriteLine($"Mining blocks with {j} TASK");
                MiningService.SetTaskCount(j);

                for (int i = 0; i < 15; i++)
                {
                    var difficulty = blockchain.Dificulty;

                    watch.Reset();
                    watch.Start();
                    blockchain.AddBlock("Alice", "-> Bob: 5 BTC");
                    watch.Stop();
                    Console.WriteLine($" time taken for block with dificulty {difficulty}: {watch.ElapsedMilliseconds} ms");

                    blockchain.PrintDificultyHistory();
                }
            }

        }

        
    }
}
