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

        private List<string> ChooseTradePairs()
        {
            var availableTradePairs = _exchangeManager.GetMarketTradePairs(permissions: new List<PermissionType> { _tradeConfiguration.PermissionType });
            var tradePairs = MenuHelper.DisplayMenu(availableTradePairs);

            if (tradePairs.Count == 0)
            {
                Console.WriteLine("Please select at least one item");
            }

            return tradePairs;
        }

        private List<string> GetTradePairsFromUser()
        {
            List<string> selectedTradePairs;

            do
            {
                selectedTradePairs = ChooseTradePairs();
            } while (selectedTradePairs.Count == 0);

            return selectedTradePairs;
        }

        public void Run()
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
                var tradePairs = GetTradePairsFromUser();
                Console.ResetColor();

                Console.WriteLine("Selected: " + string.Join(", ", tradePairs));

                loopThread = new Thread(() =>
                {
                    Task.Run(() =>
                    {
                        _TradesSubscriptionManager.SubscribeToTrades(tradePairs, _tradeConfiguration.EventType, cancellationTokenSource.Token);
                    }).Wait();
                });


                loopThread.Start();

                try
                {
                    Task.Delay(TimeSpan.FromMinutes(5), cancellationTokenSource.Token);
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
