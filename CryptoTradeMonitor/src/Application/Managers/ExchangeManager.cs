using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data.Interfaces;

namespace Application.Managers
{
    public class ExchangeManager : IExchangeManager
    {
        private readonly IExchangeRepository _exchangeRepository;

        public ExchangeManager(IExchangeRepository exchangeApiRepository)
        {
            _exchangeRepository = exchangeApiRepository;
        }

        public async Task<List<string>> GetMarketTradePairsAsync(MarketType marketType)
        {
            return await _exchangeRepository.GetMarketTradePairsAsync(marketType);
        }

        public async Task<List<string>> GetFilteredTradePairsAsync(MarketType marketType, IEnumerable<string> tradePairs)
        {
            var filteredTradePairs = (await _exchangeRepository.GetMarketTradePairsAsync(marketType))
                .Intersect(tradePairs)
                .ToList();

            return filteredTradePairs;
        }

        public async Task<List<BinanceTrade>> GetTradesAsync(List<TradePair> tradePairs, int tradeHistoryCount = 1000)
        {
            var trades = await _exchangeRepository.GetTradesAsync(tradePairs, tradeHistoryCount);

            return trades;
        }
    }
}
