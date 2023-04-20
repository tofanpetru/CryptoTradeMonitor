using Application.Interfaces;
using Common.Configuration;
using Domain.Configurations;
using Domain.Entities;
using Infrastructure.Data.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Application.Managers
{
    public class TradesSubscriptionManager : ITradesSubscriptionManager, IDisposable
    {
        private static readonly TradeConfiguration _tradeConfiguration = AppSettings<TradeConfiguration>.Instance;
        private readonly IBinanceSocketApiRequestExecutor _socketApiExecutor;
        private readonly ConcurrentDictionary<string, List<BinanceTrade>> _tradeDataStore;
        private readonly Timer _clearOldTradesTimer;

        public TradesSubscriptionManager(IBinanceSocketApiRequestExecutor socketApiExecutor)
        {
            _socketApiExecutor = socketApiExecutor;
            _clearOldTradesTimer = new Timer(ClearOldTrades,
                                             null,
                                             TimeSpan.Zero,
                                             TimeSpan.FromSeconds(_tradeConfiguration.ClearOldTradesIntervalSeconds));
            _tradeDataStore = new ConcurrentDictionary<string, List<BinanceTrade>>();
        }

        public async Task SubscribeToTradesAsync(List<string> tradePairs, string eventType, CancellationToken cancellationToken)
        {
            var tasks = new List<Task>();

            foreach (var tradePair in tradePairs)
            {
                var subscriptionTask = SubscribeToTradeAsync(tradePair, eventType, cancellationToken);
                tasks.Add(subscriptionTask);
            }

            await Task.WhenAll(tasks);
        }

        private async Task SubscribeToTradeAsync(string tradePair, string eventType, CancellationToken cancellationToken)
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

                    if (!_tradeDataStore.TryGetValue(tradePair, out var tradeList))
                    {
                        tradeList = new List<BinanceTrade>();
                        _tradeDataStore.TryAdd(tradePair, tradeList);
                    }

                    tradeList.Add(trade);

                    if (tradeList.Count > _tradeConfiguration.MaxTradeCount)
                    {
                        tradeList.RemoveRange(0, tradeList.Count - _tradeConfiguration.MaxTradeCount);
                    }

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

                if (tradeList.Count > _tradeConfiguration.MaxTradeCount)
                {
                    tradeList.RemoveRange(0, tradeList.Count - _tradeConfiguration.MaxTradeCount);
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