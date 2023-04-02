using Domain.Entities;
using Infrastructure.Data.Interfaces;
using Newtonsoft.Json;

namespace CryptoTradeMonitor.Infrastructure.Data
{
    public class ExchangeApi : IExchangeApi
    {
        private readonly List<HttpClient> _httpClients;
        private int _currentIndex = 0;

        public ExchangeApi()
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

        #region Public methods
        public async Task<List<TradePair>> GetTradePairsAsync()
        {
            var response = await ExecuteApiRequestAsync(async client => await client.GetAsync("/api/v3/exchangeInfo"));

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var exchangeInfo = JsonConvert.DeserializeObject<ExchangeInfo>(responseContent);

                var tradePairs = new List<TradePair>();
                foreach (var symbol in exchangeInfo.Symbols)
                {
                    if (symbol.IsSpotTradingAllowed)
                    {
                        tradePairs.Add(new TradePair
                        {
                            BaseAsset = symbol.BaseAsset,
                            QuoteAsset = symbol.QuoteAsset
                        });
                    }
                }

                return tradePairs;
            }

            return null;
        }

        public async Task<HttpResponseMessage> ExecuteApiRequestAsync(Func<HttpClient, Task<HttpResponseMessage>> request)
        {
            HttpResponseMessage response = null;
            int retryCount = 0;

            while (retryCount < _httpClients.Count)
            {
                var availableApiClient = await GetAvailableApiClientAsync();

                if (availableApiClient != null)
                {
                    try
                    {
                        response = await request(availableApiClient);
                        if (response.IsSuccessStatusCode)
                        {
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // Ignore exception and try the next available API
                    }
                }

                retryCount++;
            }

            return response;
        }
        #endregion

        #region Private methods
        private Task<HttpClient> GetAvailableApiClientAsync()
        {
            if (_currentIndex >= _httpClients.Count)
            {
                _currentIndex = 0;
            }

            return Task.FromResult(_httpClients[_currentIndex++]);
        }
        #endregion
    }
}
