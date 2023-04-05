using Newtonsoft.Json;

namespace Infrastructure.Data.Interfaces
{
    public interface IBinanceApiRequestExecutor
    {
        Task<HttpResponseMessage> ExecuteApiRequestAsync(Func<HttpClient, Task<HttpResponseMessage>> func);
        Task<T> ExecuteApiRequestAsync<T>(Func<HttpClient, Task<HttpResponseMessage>> func);
        Task<T> GetContentAsync<T>(HttpResponseMessage response);
        Task<T> GetContentAsync<T>(HttpResponseMessage response, params JsonConverter[] converters);
        Task<string> ExecuteApiRequestAsyncAsString(Func<HttpClient, Task<HttpResponseMessage>> func);
    }
}
