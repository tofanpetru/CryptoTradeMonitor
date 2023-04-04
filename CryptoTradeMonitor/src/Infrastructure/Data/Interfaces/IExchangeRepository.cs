using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Data.Interfaces
{
    public interface IExchangeRepository
    {
        Task<List<string>> GetMarketTradePairsAsync(List<string> symbols = null, List<PermissionType> permissions = null);
        Task<List<BinanceTrade>> GetTradesAsync(List<TradePair> tradePairs, int tradeHistoryCount);
    }
}
