namespace Application.Interfaces
{
    public interface ITradesSubscriptionManager : IDisposable
    {
        void SubscribeToTrades(List<string> tradePairs, string eventType, CancellationToken cancellationToken);
    }
}
