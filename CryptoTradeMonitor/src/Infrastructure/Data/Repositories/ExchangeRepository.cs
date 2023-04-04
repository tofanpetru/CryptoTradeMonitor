using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data.Converters;
using Infrastructure.Data.Interfaces;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Text;

namespace Infrastructure.Data.Repositories
{
    public class ExchangeRepository : IExchangeRepository
    {
        private readonly IBinanceApiRequestExecutor _requestExecutor;
        private readonly ConcurrentDictionary<string, List<string>> _cachedPairs = new();


        public ExchangeRepository(IBinanceApiRequestExecutor requestExecutor)
        {
            _requestExecutor = requestExecutor;
        }

        #region Public methods
        public async Task<List<string>> GetMarketTradePairsAsync(List<string> symbols = null, List<PermissionType> permissions = null)
        {
            var queryString = new StringBuilder();

            if (symbols?.Any() == true)
            {
                if (symbols.Count == 1)
                {
                    queryString.Append($"symbol={symbols[0]}&");
                }
                else
                {
                    queryString.Append($"symbols={string.Join(",", symbols)}&");
                }
            }

            if (permissions?.Any() == true && symbols == null)
            {
                queryString.Append($"permissions={string.Join(",", permissions)}&");
            }

            var cacheKey = queryString.ToString();

            if (_cachedPairs.TryGetValue(cacheKey, out var cachedResult))
            {
                return cachedResult;
            }

            var response = await _requestExecutor.ExecuteApiRequestAsync(async client =>
            {
                var query = cacheKey.TrimEnd('&');
                var uri = new Uri($"/api/v3/exchangeInfo?{query}", UriKind.Relative);
                return await client.GetAsync(uri);
            });

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve trade pairs: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var exchangeInfo = JsonConvert.DeserializeObject<BinanceExchangeInfo>(responseContent, new PermissionTypeConverter());

            var tradePairs = exchangeInfo.Symbols
                .Where(s => symbols == null || symbols.Contains(s.Symbol))
                .Where(s => permissions == null || s.Permissions.Any(p => permissions.Contains(p)))
                .Select(s => s.Symbol)
                .ToList();

            _cachedPairs.TryAdd(cacheKey, tradePairs);

            // Remove the cached result after 1 hour
            _ = Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromHours(1));
                _cachedPairs.TryRemove(cacheKey, out _);
            });

            return tradePairs;
        }

        public async Task<List<BinanceTrade>> GetTradesAsync(List<TradePair> tradePairs, int tradeHistoryCount = 1000)
        {
            int limit = tradeHistoryCount > 1000 ? 1000 : tradeHistoryCount;

            var tasks = tradePairs.Select(async tradePair =>
            {
                var response = await _requestExecutor.ExecuteApiRequestAsync(async client =>
                {
                    var uri = new Uri($"/api/v3/trades?symbol={tradePair.BaseAsset}{tradePair.QuoteAsset}&limit={limit}", UriKind.Relative);
                    return await client.GetAsync(uri);
                });

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to retrieve trades for {tradePair}: {response.ReasonPhrase}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<BinanceTrade>>(content);

                return result.Select(trade => MapTrade(tradePair, trade));
            }).ToList();

            var trades = (await Task.WhenAll(tasks)).SelectMany(x => x).ToList();

            return trades;
        }
        #endregion

        #region Private methods
        private BinanceTrade MapTrade(TradePair tradePair, BinanceTrade trade)
        {
            var mappedTrade = new BinanceTrade
            {
                TradePair = tradePair,
                Price = trade.Price,
                Quantity = trade.Quantity,
                IsBuyerMaker = trade.IsBuyerMaker,
                Direction = trade.Direction
            };

            return mappedTrade;
        }
        #endregion
    }
}