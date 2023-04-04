using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data.Interfaces;

namespace Application.Builders
{
    public class MarketTradePairsBuilder : IMarketTradePairsBuilder
    {
        private readonly IExchangeRepository _exchangeRepository;

        public MarketTradePairsBuilder(IExchangeRepository exchangeRepository)
        {
            _exchangeRepository = exchangeRepository;
        }
        public async Task<List<string>> BuildAsync(Action<MarketTradePairsOptions> optionsAction = null)
        {
            var options = new MarketTradePairsOptions();
            optionsAction?.Invoke(options);

            return await _exchangeRepository.GetMarketTradePairsAsync(options.Symbols, options.Permissions);
        }
    }
}
