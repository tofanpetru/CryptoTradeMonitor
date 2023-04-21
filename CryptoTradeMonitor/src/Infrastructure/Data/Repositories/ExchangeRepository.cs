using Common.Helpers;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data.Converters;
using Infrastructure.Data.Interfaces;
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
        public List<string> GetMarketTradePairs(List<string> symbols = null, List<PermissionType> permissions = null)
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

            var response = _requestExecutor.ExecuteApiRequest(client =>
            {
                var query = cacheKey.TrimEnd('&');
                var uri = new Uri($"/api/v3/exchangeInfo?{query}", UriKind.Relative);

                return AsyncHelper.RunSync(() => client.GetAsync(uri));
            });

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to retrieve trade pairs: {response.ReasonPhrase}");
            }

            var exchangeInfo = _requestExecutor.GetContent<BinanceExchangeInfo>(response, new PermissionTypeConverter());

            var tradePairs = exchangeInfo.Symbols
                .Where(s => symbols == null || symbols.Contains(s.Symbol))
                .Where(s => permissions == null || s.Permissions.Any(p => permissions.Contains(p)))
                .Select(s => s.Symbol)
                .ToList();

            _cachedPairs.TryAdd(cacheKey, tradePairs);

            // Remove the cached result after 1 hour
            _ = Task.Run( () =>
            {
                Task.Delay(TimeSpan.FromHours(1));
                _cachedPairs.TryRemove(cacheKey, out _);
            });

            return tradePairs;
        }

        public List<BinanceTrade> GetTrades(List<TradePair> tradePairs, int tradeHistoryCount = 1000)
        {
            int limit = tradeHistoryCount > 1000 ? 1000 : tradeHistoryCount;

            var tasks = tradePairs.Select(tradePair =>
            {
                var response = _requestExecutor.ExecuteApiRequest(client =>
                {
                    var uri = new Uri($"/api/v3/trades?symbol={tradePair.BaseAsset}{tradePair.QuoteAsset}&limit={limit}", UriKind.Relative);

                    return AsyncHelper.RunSync(() => client.GetAsync(uri));
                });

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to retrieve trades for {tradePair}: {response.ReasonPhrase}");
                }

                var result = _requestExecutor.GetContent<List<BinanceTrade>>(response);

                return result.Select(trade => MapTrade(tradePair, trade));
            }).ToList();

            var trades = tasks.SelectMany(x => x).ToList();

            return trades;
        }
        #endregion

        #region Private methods
        private static BinanceTrade MapTrade(TradePair tradePair, BinanceTrade trade)
        {
            var mappedTrade = new BinanceTrade
            {
                TradePair = tradePair,
                Price = trade.Price,
                Quantity = trade.Quantity,
                IsBuyerMaker = trade.IsBuyerMaker
            };

            return mappedTrade;
        }
        #endregion
    }
}