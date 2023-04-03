using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Data.Interfaces
{
    public interface IExchangeRepository
    {
        Task<List<string>> GetMarketTradePairsAsync(MarketType marketType);
        List<string> ChooseTradePairs(IEnumerable<string> tradePairs);
        Task<List<BinanceTrade>> GetTradesAsync(List<TradePair> tradePairs, int tradeHistoryCount);
    }
}
