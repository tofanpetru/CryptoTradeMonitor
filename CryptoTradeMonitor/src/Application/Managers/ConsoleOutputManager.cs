using Application.Interfaces;
using Application.Utils;
using Domain.Configurations;
using Domain.Entities;
using Domain.Enums;
using System.Collections.Concurrent;

namespace Application.Managers
{
    public class ConsoleOutputManager : IConsoleOutputManager
    {
        private readonly IExchangeManager _exchangeManager;
        private readonly ITradesSubscriptionManager _TradesSubscriptionManager;
        private static readonly TradeConfiguration _tradeConfiguration = AppSettings<TradeConfiguration>.Instance;

        private readonly OutputManager _outputManager;

        public ConsoleOutputManager(IExchangeManager exchangeManager, ITradesSubscriptionManager TradesSubscriptionManager)
        {
            _exchangeManager = exchangeManager;
            _TradesSubscriptionManager = TradesSubscriptionManager;

            _outputManager = new OutputManager();
            _outputManager.Start();
        }
        private async Task<List<string>> ChooseTradePairsAsync()
        {
            var availableTradePairs = await _exchangeManager.GetMarketTradePairsAsync(permissions: new List<PermissionType> { _tradeConfiguration.PermissionType });
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

            Console.WriteLine("Loading...");
            Console.WriteLine("Tofan Petru, 069772462");

            try
            {
                var tradePairs = await GetTradePairsFromUserAsync();
                Console.ResetColor();

                Console.WriteLine("Selected: " + string.Join(", ", tradePairs));

                loopThread = new Thread(async () =>
                {
                    await _TradesSubscriptionManager.SubscribeToTradesAsync(tradePairs, (tradePair, trade) =>
                    {
                        var color = trade.IsBuyer ? ConsoleColor.Green : ConsoleColor.Red;
                        _outputManager.OutputTrade(tradePair, trade, color);
                    }, _tradeConfiguration.EventType, cancellationTokenSource.Token);
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

        private class OutputManager
        {
            private readonly BlockingCollection<(string TradePair, BinanceTrade Trade, ConsoleColor Color)> _outputQueue;
            private readonly Thread _outputThread;

            public OutputManager()
            {
                _outputQueue = new BlockingCollection<(string, BinanceTrade, ConsoleColor)>();
                _outputThread = new Thread(ProcessOutputQueue) { IsBackground = true };
            }

            public void Start()
            {
                _outputThread.Start();
            }

            public void OutputTrade(string tradePair, BinanceTrade trade, ConsoleColor color)
            {
                _outputQueue.Add((tradePair, trade, color));
            }

            private void ProcessOutputQueue()
            {
                foreach (var output in _outputQueue.GetConsumingEnumerable())
                {
                    Console.ForegroundColor = output.Color;
                    Console.WriteLine($"{output.TradePair} - {output.Trade.TradeTime}: {output.Trade.Price} {output.Trade.Quantity}");
                    Console.ResetColor();
                }
            }
        }
    }
}
