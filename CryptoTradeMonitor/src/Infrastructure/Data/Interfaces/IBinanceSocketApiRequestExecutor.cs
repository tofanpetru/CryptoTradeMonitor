namespace Infrastructure.Data.Interfaces
{
    public interface IBinanceSocketApiRequestExecutor : IDisposable
    {
        void Connect();
        void Send(string message);
        string Receive();
        void StartReceiveLoop(CancellationToken cancellationToken);
        string Receive(string subscriptionId, TimeSpan timeout);
        bool Subscribe(string symbol, string eventType, Action<string> callback);
    }
}
