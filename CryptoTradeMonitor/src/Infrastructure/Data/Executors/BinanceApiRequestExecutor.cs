using Common.Configuration;
using Common.Helpers;
using Domain.Configurations;
using Infrastructure.Data.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace Infrastructure.Data.Executors
{
    public class BinanceApiRequestExecutor : IBinanceApiRequestExecutor
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ConcurrentDictionary<string, string> _cachedResponses = new();
        private readonly List<HttpClient> _httpClients;
        private int _currentIndex = 0;

        public BinanceApiRequestExecutor()
        {
            var httpClientsConfig = AppSettings<BinanceConfiguration>.Instance.HttpClients;
            _httpClients = new List<HttpClient>();

            foreach (var client in httpClientsConfig)
            {
                var httpClient = new HttpClient { BaseAddress = new Uri(client.BaseAddress) };
                _httpClients.Add(httpClient);
            }
        }
        #region Async methods
        public async Task<HttpResponseMessage> ExecuteApiRequestAsync(Func<HttpClient, Task<HttpResponseMessage>> func)
        {
            HttpResponseMessage response = null;
            var retries = 0;

            while (retries < _httpClients.Count)
            {
                var client = _httpClients[_currentIndex % _httpClients.Count];
                _currentIndex++;

                try
                {
                    response = await func(client);

                    if (response.IsSuccessStatusCode)
                    {
                        return response;
                    }
                }
                catch
                {
                    // ignore any exceptions thrown by the HttpClient
                }

                retries++;
            }

            throw new Exception($"Failed to execute API request after {retries} retries: {response?.ReasonPhrase}");
        }

        public async Task<T> ExecuteApiRequestAsync<T>(Func<HttpClient, Task<HttpResponseMessage>> func)
        {
            var response = await ExecuteApiRequestAsync(func);

            return await GetContentAsync<T>(response);
        }

        public async Task<T> GetContentAsync<T>(HttpResponseMessage response, params JsonConverter[] converters)
        {
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content, converters);
        }

        public async Task<T> GetContentAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(content);
        }

        public async Task<string> ExecuteApiRequestAsyncAsString(Func<HttpClient, Task<HttpResponseMessage>> func)
        {
            var cacheKey = await GetCacheKey(func);

            if (_cachedResponses.TryGetValue(cacheKey, out var cachedResponse))
            {
                return cachedResponse;
            }

            var response = await ExecuteApiRequestAsync(func);
            var content = await response.Content.ReadAsStringAsync();

            _cachedResponses.TryAdd(cacheKey, content);

            // Remove the cached result after 1 minute
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
                _cachedResponses.TryRemove(cacheKey, out _);
            });

            return content;
        }

        private async Task<string> GetCacheKey(Func<HttpClient, Task<HttpResponseMessage>> func)
        {
            var httpRequest = new HttpRequestMessage();
            var httpClient = _httpClientFactory.CreateClient();

            var response = await func(httpClient);
            var content = await response.Content.ReadAsStringAsync();

            httpRequest.Method = response.RequestMessage.Method;
            httpRequest.RequestUri = response.RequestMessage.RequestUri;
            httpRequest.Content = new StringContent(content, null, response.Content.Headers.ContentType.MediaType);

            return $"{httpRequest.Method}:{httpRequest.RequestUri}";
        }

        #endregion

        #region Sync methods
        public HttpResponseMessage ExecuteApiRequest(Func<HttpClient, HttpResponseMessage> func)
        {
            return AsyncHelper.RunSync(() => ExecuteApiRequestAsync(client => Task.FromResult(func(client))));
        }
        public T ExecuteApiRequest<T>(Func<HttpClient, HttpResponseMessage> func)
        {
            return AsyncHelper.RunSync(() => ExecuteApiRequestAsync<T>(client => Task.FromResult(func(client))));
        }

        public T GetContent<T>(HttpResponseMessage response, params JsonConverter[] converters)
        {
            return AsyncHelper.RunSync(() => GetContentAsync<T>(response, converters));
        }

        public T GetContent<T>(HttpResponseMessage response)
        {
            return AsyncHelper.RunSync(() => GetContentAsync<T>(response));
        }

        public string ExecuteApiRequestAsString(Func<HttpClient, HttpResponseMessage> func)
        {
            return AsyncHelper.RunSync(() => ExecuteApiRequestAsyncAsString(client => Task.FromResult(func(client))));
        }
        #endregion
    }
}
