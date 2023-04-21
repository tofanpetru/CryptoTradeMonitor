using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data.Interfaces;

namespace Application.Managers
{
    public class ExchangeManager : IExchangeManager
    {
        private readonly IMarketTradePairsBuilder _marketTradePairsBuilder;
        private readonly IExchangeRepository _exchangeRepository;

        public ExchangeManager(IExchangeRepository exchangeRepository, IMarketTradePairsBuilder marketTradePairsBuilder)
        {
            _exchangeRepository = exchangeRepository;
            _marketTradePairsBuilder = marketTradePairsBuilder;
        }

        public List<string> GetMarketTradePairs(List<string> symbols = null, List<PermissionType> permissions = null)
        {
            return _marketTradePairsBuilder.Build(options =>
            {
                options.Symbols = symbols;
                options.Permissions = permissions;
            });
        }

        public List<BinanceTrade> GetTrades(List<TradePair> tradePairs, int tradeHistoryCount = 1000)
        {
            var trades = _exchangeRepository.GetTrades(tradePairs, tradeHistoryCount);

            return trades;
        }
    }
}
