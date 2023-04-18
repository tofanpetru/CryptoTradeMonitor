using Domain.Entities;
using Infrastructure.Data.Interfaces;
using System.Collections.Concurrent;

namespace Infrastructure.Data.Repositories
{
    public class InMemoryTradesRepository : ITradesRepository
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<Trade>> _tradesByPair;
        private readonly int _maxTradesPerPair;

        public InMemoryTradesRepository(int maxTradesPerPair)
        {
            _tradesByPair = new ConcurrentDictionary<string, ConcurrentQueue<Trade>>();
            _maxTradesPerPair = maxTradesPerPair;
        }

        public async Task AddTradeAsync(string tradePair, Trade trade)
        {
            var trades = _tradesByPair.GetOrAdd(tradePair, _ => new ConcurrentQueue<Trade>());
            trades.Enqueue(trade);

            while (trades.Count > _maxTradesPerPair)
            {
                trades.TryDequeue(out _);
            }

            await Task.CompletedTask;
        }

        public IEnumerable<Trade> GetTrades(string tradePair)
        {
            if (_tradesByPair.TryGetValue(tradePair, out var trades))
            {
                return trades.ToList();
            }

            return Enumerable.Empty<Trade>();
        }
    }
}
