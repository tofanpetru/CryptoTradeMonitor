using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Application.Managers
{
    public class TradesSubscriptionService : ITradesSubscriptionManager, IDisposable
    {
        private readonly IBinanceSocketApiRequestExecutor _socketApiExecutor;
        private readonly ConcurrentDictionary<string, List<BinanceTrade>> _tradeDataStore;
        private readonly Timer _clearOldTradesTimer;

        public TradesSubscriptionService(IBinanceSocketApiRequestExecutor socketApiExecutor)
        {
            _socketApiExecutor = socketApiExecutor;
            _tradeDataStore = new ConcurrentDictionary<string, List<BinanceTrade>>();
            _clearOldTradesTimer = new Timer(ClearOldTrades,
                                             null,
                                             TimeSpan.Zero,
                                             TimeSpan.FromSeconds(AppSettings.Configuration.ClearOldTradesIntervalSeconds));
        }

        public async Task SubscribeToTradesAsync(List<string> tradePairs, Action<string, BinanceTrade> tradeCallback, string eventType, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            foreach (var tradePair in tradePairs)
            {
                var subscriptionTask = SubscribeToTradeAsync(tradePair, tradeCallback, eventType, cancellationToken);
                tasks.Add(subscriptionTask);
            }

            await Task.WhenAll(tasks);
        }

        private async Task SubscribeToTradeAsync(string tradePair, Action<string, BinanceTrade> tradeCallback, string eventType, CancellationToken cancellationToken)
        {
            var success = await _socketApiExecutor.SubscribeAsync(tradePair, eventType, response =>
            {
                try
                {
                    var trade = JsonConvert.DeserializeObject<BinanceTrade>(response);

                    if (trade == null)
                    {
                        Console.WriteLine($"Failed to parse trade: {response}");
                        return;
                    }

                    _tradeDataStore.AddOrUpdate(tradePair, new List<BinanceTrade>() { trade }, (key, oldValue) =>
                    {
                        oldValue.Add(trade);
                        return oldValue;
                    });

                    tradeCallback(tradePair, trade);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to parse trade: {response}. Exception: {ex}");
                }
            });

            if (!success)
            {
                Console.WriteLine($"Failed to subscribe to {tradePair}@{eventType}.");
            }

            // Run StartReceiveLoop on a separate thread
            _ = Task.Run(() => _socketApiExecutor.StartReceiveLoop(cancellationToken), cancellationToken);
        }

        private void ClearOldTrades(object state)
        {
            foreach (var tradePair in _tradeDataStore.Keys)
            {
                var tradeList = _tradeDataStore[tradePair];

                if (tradeList.Count > AppSettings.Configuration.MaxTradeCount)
                {
                    tradeList.RemoveRange(0, tradeList.Count - AppSettings.Configuration.MaxTradeCount);
                }
            }
        }

        public void Dispose()
        {
            _clearOldTradesTimer?.Dispose();
            _socketApiExecutor?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
