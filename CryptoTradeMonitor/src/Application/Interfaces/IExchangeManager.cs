using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IExchangeManager
    {
        Task<List<string>> GetMarketTradePairsAsync(List<string> symbols = null, List<PermissionType> permissions = null);
        Task<List<BinanceTrade>> GetTradesAsync(List<TradePair> tradePairs, int tradeHistoryCount = 1000);
    }
}
