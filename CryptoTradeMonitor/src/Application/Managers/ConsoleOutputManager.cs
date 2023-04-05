using Application.Interfaces;
using Domain.Enums;
using System.Runtime.InteropServices;

namespace Application.Managers
{
    public class ConsoleOutputManager : IConsoleOutputManager
    {
        private readonly IExchangeManager _exchangeManager;

        public ConsoleOutputManager(IExchangeManager exchangeManager)
        {
            _exchangeManager = exchangeManager;
        }


        public List<T> DisplayMenu<T>(List<T> items, int itemsPerPage = 5)
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


        private async Task<List<string>> ChooseTradePairsAsync()
        {
            Console.WriteLine("Please enter the desired trade pairs, separated by a comma or space:");
            var tradePairs = Console.ReadLine()?.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();

            var availableTradePairs = await _exchangeManager.GetMarketTradePairsAsync(permissions: new List<PermissionType> { PermissionType.SPOT });
            tradePairs = tradePairs.Intersect(availableTradePairs).ToList();

            if (tradePairs.Count == 0)
            {
                Console.WriteLine("No valid trade pairs were selected. Available trade pairs:");
                Console.WriteLine(string.Join(", ", availableTradePairs));
            }

            return tradePairs;
        }

        private async Task<List<string>> GetTradePairsFromUserAsync()
        {
            List<string> selectedTradePairs;

            do
            {
                selectedTradePairs = await ChooseTradePairsAsync();
            } while (selectedTradePairs.Count == 0);

            return selectedTradePairs;
        }

        public async Task Run()
        {
            var tradePairs = await GetTradePairsFromUserAsync();

            Console.WriteLine(tradePairs.Count);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.ReadLine();
            }
        }
    }
}
