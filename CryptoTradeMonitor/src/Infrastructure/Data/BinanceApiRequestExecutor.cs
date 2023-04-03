using Infrastructure.Data.Interfaces;

namespace Infrastructure.Data
{
    public class BinanceApiRequestExecutor : IBinanceApiRequestExecutor
    {
        private readonly List<HttpClient> _httpClients;
        private int _currentIndex = 0;

        public BinanceApiRequestExecutor()
        {
            _httpClients = new List<HttpClient>
            {
                new HttpClient { BaseAddress = new Uri("https://api.binance.com") },
                new HttpClient { BaseAddress = new Uri("https://api1.binance.com") },
                new HttpClient { BaseAddress = new Uri("https://api2.binance.com") },
                new HttpClient { BaseAddress = new Uri("https://api3.binance.com") },
                new HttpClient { BaseAddress = new Uri("https://api4.binance.com") }
            };
        }

        public async Task<HttpResponseMessage> ExecuteApiRequestAsync(Func<HttpClient, Task<HttpResponseMessage>> request)
        {
            const int maxRetries = 3;
            var retries = 0;

            while (retries < maxRetries)
            {
                var client = _httpClients[_currentIndex];
                try
                {
                    return await request(client);
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("Connection refused"))
                {
                    Console.WriteLine($"Request failed with error '{ex.Message}'. Retrying...");
                    _currentIndex = (_currentIndex + 1) % _httpClients.Count;
                    retries++;
                }
            }

            throw new Exception("Failed to execute API request after max retries.");
        }
    }

}
