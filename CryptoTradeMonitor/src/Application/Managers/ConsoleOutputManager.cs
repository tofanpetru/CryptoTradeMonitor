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

        public async Task Run()
        {
            Console.WriteLine("Please enter the desired market type (Spot, Futures, etc.):");
            var marketTypeStr = Console.ReadLine();
            if (!Enum.TryParse<MarketType>(marketTypeStr, true, out var marketType))
            {
                Console.WriteLine($"Invalid market type: {marketTypeStr}");
                return;
            }

            var tradePairs = await _exchangeManager.GetMarketTradePairsAsync(MarketType.Futures);
            Console.WriteLine($"Available trade pairs for {marketType}: {string.Join(", ", tradePairs)}");

            Console.WriteLine("Please enter the desired trade pairs, separated by a comma or space:");
            var tradePairsStr = Console.ReadLine();
            var selectedTradePairs = tradePairsStr.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            var filteredTradePairs = await _exchangeManager.GetFilteredTradePairsAsync(marketType, selectedTradePairs);
            Console.WriteLine($"Filtered trade pairs for {marketType}: {string.Join(", ", filteredTradePairs)}");

            var trades = await _exchangeManager.GetTradesAsync(null);
            foreach (var trade in trades)
            {
                Console.WriteLine(trade);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.ReadLine();
            }
        }
    }
}
