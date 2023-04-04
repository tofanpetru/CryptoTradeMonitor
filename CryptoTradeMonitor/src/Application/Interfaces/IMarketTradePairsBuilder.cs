using Domain.Entities;

namespace Application.Interfaces
{
    public interface IMarketTradePairsBuilder
    {
        Task<List<string>> BuildAsync(Action<MarketTradePairsOptions> optionsAction = null);
    }
}
