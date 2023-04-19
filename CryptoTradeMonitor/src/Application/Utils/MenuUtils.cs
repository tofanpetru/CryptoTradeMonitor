namespace Application.Utils
{
    public static class MenuUtils
    {

        public static List<T> DisplayMenu<T>(List<T> items, int itemsPerPage = 9)
        {
            int currentPage = 0;
            int totalPages = (int)Math.Ceiling(items.Count / (double)itemsPerPage);
            List<int> selectedItemIndexes = new();
            int currentSelectedItem = 0;
            bool shouldClearConsole = true;

            while (true)
            {
                if (shouldClearConsole)
                {
                    Console.Clear();
                    Console.WriteLine($"Page {currentPage + 1}/{totalPages}");
                    Console.WriteLine();

                    for (int i = currentPage * itemsPerPage; i < (currentPage + 1) * itemsPerPage && i < items.Count; i++)
                    {
                        if (i == currentSelectedItem)
                        {
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }

                        if (selectedItemIndexes.Contains(i))
                        {
                            Console.Write("* ");
                        }
                        else
                        {
                            Console.Write("  ");
                        }

                        Console.WriteLine($"{i + 1}. {items[i]}");

                        if (i == currentSelectedItem)
                        {
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine("Use arrow keys to navigate, Space to select/deselect, and Enter to finish.");
                    Console.WriteLine("Up/Down arrows move through items. Left/Right arrows change pages.");
                    Console.WriteLine("Reaching the first/last item on a page will automatically move to the previous/next page.");
                }

                shouldClearConsole = true;
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (currentSelectedItem > 0)
                        {
                            currentSelectedItem--;
                        }
                        else
                        {
                            shouldClearConsole = false;
                        }
                        if (currentSelectedItem < currentPage * itemsPerPage && currentPage > 0)
                        {
                            currentPage--;
                            currentSelectedItem = (currentPage + 1) * itemsPerPage - 1;
                        }
                        break;

                    case ConsoleKey.DownArrow:
                        if (currentSelectedItem < items.Count - 1 && !(currentSelectedItem == items.Count - 1 && currentPage == totalPages - 1))
                        {
                            currentSelectedItem++;
                        }
                        else
                        {
                            shouldClearConsole = false;
                        }
                        if (currentSelectedItem >= (currentPage + 1) * itemsPerPage && currentPage < totalPages - 1)
                        {
                            currentPage++;
                        }
                        break;

                    case ConsoleKey.LeftArrow:
                        if (currentPage > 0)
                        {
                            currentPage--;
                            currentSelectedItem = currentPage * itemsPerPage;
                        }
                        else
                        {
                            shouldClearConsole = false;
                        }
                        break;

                    case ConsoleKey.RightArrow:
                        if (currentPage < totalPages - 1)
                        {
                            currentPage++;
                            currentSelectedItem = currentPage * itemsPerPage;
                        }
                        else
                        {
                            shouldClearConsole = false;
                        }
                        break;

                    case ConsoleKey.Spacebar:
                        if (selectedItemIndexes.Contains(currentSelectedItem))
                        {
                            selectedItemIndexes.Remove(currentSelectedItem);
                        }
                        else
                        {
                            selectedItemIndexes.Add(currentSelectedItem);
                        }
                        break;

                    case ConsoleKey.Enter:
                        return selectedItemIndexes.Select(i => items[i]).ToList();
                }
            }
        }

    }
}
