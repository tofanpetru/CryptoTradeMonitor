using Application.Interfaces;
using Common.Configuration;
using Domain.Configurations;
using Domain.Entities;
using Infrastructure.Data.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

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

        public void SubscribeToTrades(List<string> tradePairs, string eventType, CancellationToken cancellationToken)
        {
            foreach (var tradePair in tradePairs)
            {
                SubscribeToTrade(tradePair, eventType, cancellationToken);
            }
        }

        private void SubscribeToTrade(string tradePair, string eventType, CancellationToken cancellationToken)
        {
            var success = _socketApiExecutor.Subscribe(tradePair, eventType, response =>
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