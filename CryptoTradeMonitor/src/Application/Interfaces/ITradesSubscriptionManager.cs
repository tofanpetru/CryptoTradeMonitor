namespace Application.Interfaces
{
    public interface ITradesSubscriptionManager : IDisposable
    {
        Task SubscribeToTradesAsync(List<string> tradePairs, string eventType, CancellationToken cancellationToken);
    }
}
