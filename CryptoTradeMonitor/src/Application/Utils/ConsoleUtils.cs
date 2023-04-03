namespace Application.Utils
{
    public static class ConsoleUtils
    {
        public static List<T> SelectOptions<T>(List<T> options)
        {
            List<T> selectedOptions = new();
            int selectedOptionIndex = 0;
            ConsoleKeyInfo keyInfo;

            do
            {
                Console.Clear();
                Console.WriteLine("Select one or more options (press SPACE to select and ENTER to continue):");

                for (int i = 0; i < options.Count; i++)
                {
                    Console.Write(i == selectedOptionIndex ? "> " : "  ");
                    Console.ForegroundColor = selectedOptions.Contains(options[i]) ? ConsoleColor.Green : Console.ForegroundColor;
                    Console.WriteLine($"[{(selectedOptions.Contains(options[i]) ? 'X' : ' ')}] {options[i]}");
                    Console.ResetColor();
                }

                keyInfo = Console.ReadKey();

                switch (keyInfo.Key)
                {
                    case ConsoleKey.DownArrow:
                        selectedOptionIndex = Math.Min(selectedOptionIndex + 1, options.Count - 1);
                        break;

                    case ConsoleKey.UpArrow:
                        selectedOptionIndex = Math.Max(selectedOptionIndex - 1, 0);
                        break;

                    case ConsoleKey.Spacebar:
                        if (!selectedOptions.Contains(options[selectedOptionIndex]))
                        {
                            selectedOptions.Add(options[selectedOptionIndex]);
                        }
                        else
                        {
                            selectedOptions.Remove(options[selectedOptionIndex]);
                        }
                        break;
                }

            } while (keyInfo.Key != ConsoleKey.Enter);

            Console.Clear();
            Console.WriteLine("You selected the following options:");

            foreach (T option in selectedOptions)
            {
                Console.WriteLine(option);
            }

            return selectedOptions;
        }
    }
}
