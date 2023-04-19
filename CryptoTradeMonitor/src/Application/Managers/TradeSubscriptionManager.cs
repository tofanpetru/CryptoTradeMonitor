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
        private readonly List<Task> _subscriptionTasks;

        public TradesSubscriptionService(IBinanceSocketApiRequestExecutor socketApiExecutor)
        {
            _socketApiExecutor = socketApiExecutor;
            _tradeDataStore = new ConcurrentDictionary<string, List<BinanceTrade>>();
            _subscriptionTasks = new List<Task>();
            _clearOldTradesTimer = new Timer(ClearOldTrades, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        public async Task SubscribeToTradesAsync(List<string> tradePairs, Action<string, BinanceTrade> tradeCallback, CancellationToken cancellationToken, string eventType = "trade")
        {
            var tasks = new List<Task>();

            foreach (var tradePair in tradePairs)
            {
                var subscriptionTask = SubscribeToTradeAsync(tradePair, tradeCallback, cancellationToken, eventType);
                tasks.Add(subscriptionTask);
            }

            await Task.WhenAll(tasks);
        }

        private async Task SubscribeToTradeAsync(string tradePair,
                                                 Action<string, BinanceTrade> tradeCallback,
                                                 CancellationToken cancellationToken,
                                                 string eventType)
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

            await _socketApiExecutor.StartReceiveLoop(cancellationToken);
        }

        private void ClearOldTrades(object state)
        {
            foreach (var tradePair in _tradeDataStore.Keys)
            {
                var tradeList = _tradeDataStore[tradePair];

                if (tradeList.Count > 10000)
                {
                    tradeList.RemoveRange(0, tradeList.Count - 10000);
                }
            }
        }

        public void Dispose()
        {
            _clearOldTradesTimer?.Dispose();
            _socketApiExecutor?.Dispose();
        }
    }
}
