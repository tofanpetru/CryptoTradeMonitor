using Application.Interfaces;
using Application.Utils;
using Domain.Entities;
using Domain.Enums;
using System.Collections.Concurrent;

namespace Application.Managers
{
    public class ConsoleOutputManager : IConsoleOutputManager
    {
        private readonly IExchangeManager _exchangeManager;
        private readonly ITradesSubscriptionManager _tradesSubscriptionService;
        private readonly ConcurrentQueue<(string TradePair, BinanceTrade Trade)> _tradeQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _monitorTradesTask;

        public ConsoleOutputManager(IExchangeManager exchangeManager, ITradesSubscriptionManager tradesSubscriptionService)
        {
            _exchangeManager = exchangeManager;
            _tradesSubscriptionService = tradesSubscriptionService;
            _tradeQueue = new ConcurrentQueue<(string, BinanceTrade)>();
            _cancellationTokenSource = new CancellationTokenSource();
            _monitorTradesTask = MonitorTradesAsync(_cancellationTokenSource.Token);
        }

        private async Task<List<string>> ChooseTradePairsAsync()
        {
            var availableTradePairs = await _exchangeManager.GetMarketTradePairsAsync(permissions: new List<PermissionType> { PermissionType.SPOT });
            var tradePairs = MenuUtils.DisplayMenu(availableTradePairs);

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

            try
            {
                var tradePairs = await GetTradePairsFromUserAsync();

                Console.WriteLine("Selected: " + string.Join(", ", tradePairs));

                loopThread = new Thread(async () =>
                {
                    await _tradesSubscriptionService.SubscribeToTradesAsync(tradePairs, (tradePair, trade) =>
                    {
                        var color = trade.IsBuyer ? ConsoleColor.Green : ConsoleColor.Red;

                        Console.ForegroundColor = color;
                        Console.WriteLine($"{tradePair} - {trade.TradeTime}: {trade.Price} {trade.Quantity}");
                        Console.ResetColor();
                    }, AppSettings.Configuration.EventType, cancellationTokenSource.Token);
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
                await _monitorTradesTask;
            }
        }


        private async Task MonitorTradesAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_tradeQueue.TryDequeue(out var tradeInfo))
                {
                    var (tradePair, trade) = tradeInfo;
                    var color = trade.IsBuyer ? ConsoleColor.Green : ConsoleColor.Red;

                    Console.ForegroundColor = color;
                    Console.WriteLine($"{tradePair} - {trade.TradeTime}: {trade.Price} {trade.Quantity}");
                    Console.ResetColor();
                }
                else
                {
                    await Task.Delay(100, cancellationToken);
                }
            }
        }
    }
}
