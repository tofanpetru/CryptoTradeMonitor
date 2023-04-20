using Domain.Entities;

namespace Application.Interfaces
{
    public interface IMarketTradePairsBuilder
    {
        List<string> Build(Action<MarketTradePairsOptions> optionsAction = null);
    }
}
