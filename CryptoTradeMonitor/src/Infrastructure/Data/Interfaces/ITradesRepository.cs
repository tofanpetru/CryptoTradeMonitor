using Domain.Entities;

namespace Infrastructure.Data.Interfaces
{
    public interface ITradesRepository
    {
        void AddTrade(string tradePair, BinanceTrade trade);
        IEnumerable<BinanceTrade> GetTrades(string tradePair);
    }
}
