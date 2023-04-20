using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Data.Interfaces
{
    public interface IExchangeRepository
    {
        List<string> GetMarketTradePairs(List<string> symbols = null, List<PermissionType> permissions = null);
        List<BinanceTrade> GetTrades(List<TradePair> tradePairs, int tradeHistoryCount);
    }
}
