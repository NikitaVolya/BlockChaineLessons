


namespace BlockChaine.Services
{
    public class UserInterfaceService
    {
        private class OptionModel
        {
            public string Text { get; set; }
            public Action Action { get; set; }
        }

        private int _index;
        private List<OptionModel> _options;

        public UserInterfaceService(List<(string, Action)> options, int default_index = 0)
        {
            _options = options.Select(op => new OptionModel { Text = op.Item1, Action = op.Item2 }).ToList();
            _index = default_index;
        }

        public void ShowOptionDialog()
        {
            while (true)
            {
                Console.Clear();

                for (int i = 0; i < _options.Count; i++)
                {

                    Console.Write((i == _index ? "[ " : "  "));

                    Console.Write(_options[i].Text);

                    Console.WriteLine((i == _index ? " ]" : "  "));
                }

                ConsoleKeyInfo key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        _index--;
                        if (_index < 0)
                            _index = _options.Count - 1;
                        break;

                    case ConsoleKey.DownArrow:
                        _index++;
                        if (_index >= _options.Count)
                            _index = 0;
                        break;

                    case ConsoleKey.Enter:
                        Console.Clear();
                        _options[_index].Action.Invoke();
                        return;
                }

            }
        }
    }
}
