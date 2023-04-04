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
