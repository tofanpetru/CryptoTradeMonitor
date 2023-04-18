using Domain.Entities;

namespace Infrastructure.Data.Interfaces
{
    public interface ITradesRepository
    {
        Task AddTradeAsync(string tradePair, Trade trade);
        IEnumerable<Trade> GetTrades(string tradePair);
    }
}
