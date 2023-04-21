using Domain.Entities;
using Infrastructure.Data.Interfaces;
using System.Collections.Concurrent;

namespace Infrastructure.Data.Repositories
{
    public class InMemoryTradesRepository : ITradesRepository
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<BinanceTrade>> _tradesByPair;
        private readonly int _maxTradesPerPair;

        public InMemoryTradesRepository(int maxTradesPerPair)
        {
            _tradesByPair = new ConcurrentDictionary<string, ConcurrentQueue<BinanceTrade>>();
            _maxTradesPerPair = maxTradesPerPair;
        }

        public void AddTrade(string tradePair, BinanceTrade trade)
        {
            var trades = _tradesByPair.GetOrAdd(tradePair, _ => new ConcurrentQueue<BinanceTrade>());
            trades.Enqueue(trade);

            while (trades.Count > _maxTradesPerPair)
            {
                trades.TryDequeue(out _);
            }
        }

        public IEnumerable<BinanceTrade> GetTrades(string tradePair)
        {
            if (_tradesByPair.TryGetValue(tradePair, out var trades))
            {
                return trades.ToList();
            }

            return Enumerable.Empty<BinanceTrade>();
        }
    }
}
