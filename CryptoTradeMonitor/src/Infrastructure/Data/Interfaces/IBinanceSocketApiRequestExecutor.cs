namespace Infrastructure.Data.Interfaces
{
    public interface IBinanceSocketApiRequestExecutor : IDisposable
    {
        Task ConnectAsync();
        Task SendAsync(string message);
        Task<string> ReceiveAsync();
    }
}
