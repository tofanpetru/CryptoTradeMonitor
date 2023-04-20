using Newtonsoft.Json;

namespace Infrastructure.Data.Interfaces
{
    public interface IBinanceApiRequestExecutor
    {
        HttpResponseMessage ExecuteApiRequest(Func<HttpClient, Task<HttpResponseMessage>> func);
        T ExecuteApiRequest<T>(Func<HttpClient, Task<HttpResponseMessage>> func);
        T GetContent<T>(HttpResponseMessage response);
        T GetContent<T>(HttpResponseMessage response, params JsonConverter[] converters);
        string ExecuteApiRequestAsyncAsString(Func<HttpClient, Task<HttpResponseMessage>> func);
    }
}
