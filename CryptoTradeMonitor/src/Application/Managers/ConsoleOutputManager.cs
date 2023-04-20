using Application.Interfaces;
using Common.Configuration;
using Common.Helpers;
using Domain.Configurations;
using Domain.Enums;

namespace Application.Managers
{
    public class ConsoleOutputManager : IConsoleOutputManager
    {
        private readonly IExchangeManager _exchangeManager;
        private readonly ITradesSubscriptionManager _TradesSubscriptionManager;
        private static readonly TradeConfiguration _tradeConfiguration = AppSettings<TradeConfiguration>.Instance;

        public ConsoleOutputManager(IExchangeManager exchangeManager, ITradesSubscriptionManager TradesSubscriptionManager)
        {
            _exchangeManager = exchangeManager;
            _TradesSubscriptionManager = TradesSubscriptionManager;
        }
        private async Task<List<string>> ChooseTradePairsAsync()
        {
            var availableTradePairs = await _exchangeManager.GetMarketTradePairsAsync(permissions: new List<PermissionType> { _tradeConfiguration.PermissionType });
            var tradePairs = MenuHelper.DisplayMenu(availableTradePairs);

            if (tradePairs.Count == 0)
            {
                Console.WriteLine("Please select at least one item");
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

        public async Task RunAsync()
        {
            CancellationTokenSource cancellationTokenSource = new();
            Thread loopThread = null;

            Console.CancelKeyPress += (sender, e) =>
            {
                Console.WriteLine("Closing application...");
                e.Cancel = true; // Prevents the application from closing immediately
                cancellationTokenSource?.Cancel();
            };

            Console.WriteLine("Loading...");
            Console.WriteLine("Tofan Petru, 069772462");

            try
            {
                var tradePairs = await GetTradePairsFromUserAsync();
                Console.ResetColor();

                Console.WriteLine("Selected: " + string.Join(", ", tradePairs));

                loopThread = new Thread(() =>
                {
                    Task.Run(async () =>
                    {
                        await _TradesSubscriptionManager.SubscribeToTradesAsync(tradePairs, _tradeConfiguration.EventType, cancellationTokenSource.Token);
                    }).Wait();
                });


                loopThread.Start();

                try
                {
                    await Task.Delay(TimeSpan.FromMinutes(5), cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Task was canceled. Waiting for task completion");
                }

                Console.WriteLine("Press any key to close the application...");
                Console.ReadKey();
            }
            finally
            {
                cancellationTokenSource?.Cancel();
                loopThread?.Join();
            }
        }
    }
}
