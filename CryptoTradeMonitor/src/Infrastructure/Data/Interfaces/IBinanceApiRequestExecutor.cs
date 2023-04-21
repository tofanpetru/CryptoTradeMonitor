using Newtonsoft.Json;

namespace Infrastructure.Data.Interfaces
{
    public interface IBinanceApiRequestExecutor
    {
        HttpResponseMessage ExecuteApiRequest(Func<HttpClient, HttpResponseMessage> func);
        T ExecuteApiRequest<T>(Func<HttpClient, HttpResponseMessage> func);
        T GetContent<T>(HttpResponseMessage response, params JsonConverter[] converters);
        T GetContent<T>(HttpResponseMessage response);
        string ExecuteApiRequestAsString(Func<HttpClient, HttpResponseMessage> func);
    }
}
