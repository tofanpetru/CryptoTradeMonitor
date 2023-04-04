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

        public async Task<List<string>> GetMarketTradePairsAsync(List<string> symbols = null, List<PermissionType> permissions = null)
        {
            return await _marketTradePairsBuilder.BuildAsync(options =>
            {
                options.Symbols = symbols;
                options.Permissions = permissions;
            });
        }

        public async Task<List<BinanceTrade>> GetTradesAsync(List<TradePair> tradePairs, int tradeHistoryCount = 1000)
        {
            var trades = await _exchangeRepository.GetTradesAsync(tradePairs, tradeHistoryCount);

            return trades;
        }
    }
}
