namespace Infrastructure.Data.Interfaces
{
    public interface IBinanceSocketApiRequestExecutor : IDisposable
    {
        Task ConnectAsync();
        Task SendAsync(string message);
        Task<string> ReceiveAsync();
        Task StartReceiveLoop(CancellationToken cancellationToken);
        Task<string> ReceiveAsync(string subscriptionId, TimeSpan timeout);
        Task<bool> SubscribeAsync(string symbol, string eventType, Action<string> callback);
    }
}
