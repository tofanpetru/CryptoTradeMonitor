using Domain.Entities;

namespace Application.Interfaces
{
    public interface ITradesSubscriptionManager : IDisposable
    {
        Task SubscribeToTradesAsync(List<string> tradePairs, Action<string, BinanceTrade> tradeCallback, CancellationToken cancellationToken, string eventType = "trade");
    }
}
