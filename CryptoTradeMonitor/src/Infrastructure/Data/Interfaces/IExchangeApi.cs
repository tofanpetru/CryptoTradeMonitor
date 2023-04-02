using Domain.Entities;

namespace Infrastructure.Data.Interfaces
{
    public interface IExchangeApi
    {
        Task<HttpResponseMessage> ExecuteApiRequestAsync(Func<HttpClient, Task<HttpResponseMessage>> request);
    }
}
