using Domain.Entities;
using Domain.Enums;

namespace Application.Interfaces
{
    public interface IExchangeManager
    {
        List<string> GetMarketTradePairs(List<string> symbols = null, List<PermissionType> permissions = null);
        List<BinanceTrade> GetTrades(List<TradePair> tradePairs, int tradeHistoryCount = 1000);
    }
}
