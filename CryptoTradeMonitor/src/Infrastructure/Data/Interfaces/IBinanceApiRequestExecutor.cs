namespace Infrastructure.Data.Interfaces
{
    public interface IBinanceApiRequestExecutor
    {
        Task<HttpResponseMessage> ExecuteApiRequestAsync(Func<HttpClient, Task<HttpResponseMessage>> request);
    }
}
