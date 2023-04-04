using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data.Interfaces;
using Newtonsoft.Json;
using System.Text;

namespace Infrastructure.Data.Repositories
{
    public class ExchangeRepository : IExchangeRepository
    {
        private readonly IBinanceApiRequestExecutor _requestExecutor;

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
                queryString.Append($"symbols={string.Join(",", symbols)}&");
            }

            if (permissions?.Any() == true)
            {
                queryString.Append($"permissions={string.Join(",", permissions)}&");
            }

            var query = queryString.ToString().TrimEnd('&');
            var uri = new Uri($"/api/v3/exchangeInfo?{query}", UriKind.Relative);

            var response = await _requestExecutor.ExecuteApiRequestAsync(async client =>
            {
                return await client.GetAsync(uri);
            });

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                throw new Exception($"Failed to retrieve trade pairs: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var exchangeInfo = JsonConvert.DeserializeObject<BinanceExchangeInfo>(responseContent);

            var tradePairs = exchangeInfo.Symbols
                .Where(s => symbols == null || symbols.Contains(s.Symbol))
                .Where(s => permissions == null || s.Permissions.Any(p => permissions.Contains(p)))
                .Select(s => s.Symbol)
                .ToList();

            return tradePairs;
        }

        public List<string> ChooseTradePairs(IEnumerable<string> tradePairs)
        {
            if (tradePairs?.Any() != true)
            {
                Console.WriteLine("Please enter the desired trade pairs, separated by a comma or space:");
                tradePairs = Console.ReadLine().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return tradePairs.ToList();
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