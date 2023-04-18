using Domain.Entities;

namespace Infrastructure.Data.Interfaces
{
    public interface ITradesRepository
    {
        Task AddTradeAsync(string tradePair, BinanceTrade trade);
        IEnumerable<BinanceTrade> GetTrades(string tradePair);
    }
}
