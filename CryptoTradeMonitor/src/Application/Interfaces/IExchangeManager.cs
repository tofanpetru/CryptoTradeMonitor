using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IExchangeManager
    {
        Task<List<string>> GetMarketTradePairsAsync(MarketType marketType);
        Task<List<string>> GetFilteredTradePairsAsync(MarketType marketType, IEnumerable<string> tradePairs);
        Task<List<BinanceTrade>> GetTradesAsync(List<TradePair> tradePairs, int tradeHistoryCount = 1000);
    }
}
